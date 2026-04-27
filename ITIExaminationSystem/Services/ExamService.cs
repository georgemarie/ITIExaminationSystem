using Dapper;
using ITIExaminationSystem.Interfaces;
using ITIExaminationSystem.Models;
using ITIExaminationSystem.ViewModels;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ITIExaminationSystem.Services
{
    /// <summary>
    /// Handles all exam-related operations:
    /// - Generating exams via stored procedures
    /// - Loading + RANDOMIZING questions per student
    /// - Preventing re-submission (IsSubmitted flag)
    /// - Auto score calculation via SP_CorrectExam
    /// </summary>
    public class ExamService : IExamService
    {
        private readonly IConfiguration _config;

        // Default exam duration in minutes (can be moved to appsettings later)
        private const int DefaultDurationMinutes = 30;

        public ExamService(IConfiguration config)
        {
            _config = config;
        }

        private SqlConnection GetConnection() =>
            new SqlConnection(_config.GetConnectionString("DefaultConnection"));

        /// <summary>
        /// Calls SP_GenerateExam to create a new exam for the course.
        /// Returns the new Exam_ID.
        /// </summary>
        public async Task<int> GenerateExamAsync(int courseId)
        {
            using var conn = GetConnection();

            var p = new DynamicParameters();
            p.Add("@Exam_Name", $"Exam_{DateTime.Now.Ticks}");
            p.Add("@Course_ID", courseId);
            p.Add("@Total_Degree", 100);
            p.Add("@Num_MCQ", 5);
            p.Add("@Num_TF", 5);

            int examId = await conn.ExecuteScalarAsync<int>("SP_GenerateExam", p, commandType: CommandType.StoredProcedure);

            // Mark exam as open
            await conn.ExecuteAsync("UPDATE Exam SET IsOpened = 1 WHERE Exam_ID = @Id", new { Id = examId });

            return examId;
        }

        /// <summary>
        /// Enrolls the student in the exam if not already enrolled.
        /// Uses IsSubmitted = 0 to allow re-entry before submission.
        /// </summary>
        public async Task EnrollStudentAsync(int studentId, int examId)
        {
            using var conn = GetConnection();

            const string sql = @"
                IF NOT EXISTS (SELECT 1 FROM Student_Exam WHERE St_ID = @StID AND Exam_ID = @ExamID)
                BEGIN
                    INSERT INTO Student_Exam (St_ID, Exam_ID, Grade, IsSubmitted, StartedAt)
                    VALUES (@StID, @ExamID, NULL, 0, @StartedAt)
                END";

            await conn.ExecuteAsync(sql, new { StID = studentId, ExamID = examId, StartedAt = DateTime.UtcNow });
        }

        /// <summary>
        /// Loads the exam questions with their choices, then SHUFFLES them.
        /// Each student gets a different question order (randomization feature).
        /// </summary>
        public async Task<TakeExamViewModel> GetExamForStudentAsync(int examId)
        {
            using var conn = GetConnection();

            // Get exam metadata
            const string examSql = @"
                SELECT e.Exam_ID, e.Exam_Name, c.Course_Name
                FROM Exam e
                INNER JOIN Course c ON e.Course_ID = c.Course_ID
                WHERE e.Exam_ID = @ExamId";

            var examInfo = await conn.QueryFirstOrDefaultAsync(examSql, new { ExamId = examId });

            // Load questions for this exam
            const string questionSql = @"
                SELECT q.Q_ID, q.Q_Text, q.Q_Type
                FROM Question q
                INNER JOIN Exam_Question eq ON q.Q_ID = eq.Q_ID
                WHERE eq.Exam_ID = @ExamId";

            var questions = (await conn.QueryAsync<Question>(questionSql, new { ExamId = examId })).ToList();

            // Load choices for each question
            foreach (var q in questions)
            {
                if (q.Q_Type == "MCQ")
                {
                    var choices = await conn.QueryAsync<Choice>("SELECT * FROM Choice WHERE Q_ID = @QID", new { QID = q.Q_ID });
                    q.Choices = choices.ToList();

                    // Randomize choice order too (prevents pattern memorization)
                    q.Choices = q.Choices.OrderBy(_ => Guid.NewGuid()).ToList();
                }
                else
                {
                    // True/False questions get fixed choices
                    q.Choices = new List<Choice>
                    {
                        new Choice { Choice_Code = "True",  Choice_Text = "True" },
                        new Choice { Choice_Code = "False", Choice_Text = "False" }
                    };
                }
            }

            // Randomize question order per attempt
            var shuffledQuestions = questions.OrderBy(_ => Guid.NewGuid()).ToList();

            return new TakeExamViewModel
            {
                ExamId = examId,
                ExamName = examInfo?.Exam_Name ?? "Exam",
                CourseName = examInfo?.Course_Name ?? "",
                DurationMinutes = DefaultDurationMinutes,
                StartedAt = DateTime.UtcNow,
                Questions = shuffledQuestions
            };
        }

        /// <summary>
        /// Checks the IsSubmitted flag for this student + exam pair.
        /// Prevents re-taking a submitted exam.
        /// </summary>
        public async Task<bool> HasStudentSubmittedAsync(int studentId, int examId)
        {
            using var conn = GetConnection();

            const string sql = @"
                SELECT ISNULL(IsSubmitted, 0)
                FROM Student_Exam
                WHERE St_ID = @StId AND Exam_ID = @ExamId";

            int isSubmitted = await conn.ExecuteScalarAsync<int>(sql, new { StId = studentId, ExamId = examId });
            return isSubmitted == 1;
        }

        /// <summary>
        /// Saves a single student answer via the existing stored procedure.
        /// </summary>
        public async Task SaveAnswerAsync(int studentId, int examId, int questionId, string answer)
        {
            using var conn = GetConnection();

            var p = new DynamicParameters();
            p.Add("@St_ID", studentId);
            p.Add("@Exam_ID", examId);
            p.Add("@Q_ID", questionId);
            p.Add("@St_Ans", answer);

            await conn.ExecuteAsync("SP_SubmitStudentAnswer", p, commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// Marks the exam as submitted, calls SP_CorrectExam for auto-grading,
        /// and returns the result ViewModel.
        /// </summary>
        public async Task<ExamResultViewModel> CorrectExamAsync(int studentId, int examId)
        {
            using var conn = GetConnection();

            // Mark as submitted — prevents re-entry (IsSubmitted feature)
            const string submitSql = @"
                UPDATE Student_Exam
                SET IsSubmitted = 1, SubmittedAt = @SubmittedAt
                WHERE St_ID = @StId AND Exam_ID = @ExamId";

            await conn.ExecuteAsync(submitSql, new
            {
                StId = studentId,
                ExamId = examId,
                SubmittedAt = DateTime.UtcNow
            });

            // Call existing correction procedure
            decimal grade = 0;
            try
            {
                var p = new DynamicParameters();
                p.Add("@St_ID", studentId);
                p.Add("@Exam_ID", examId);
                grade = await conn.ExecuteScalarAsync<decimal>("SP_CorrectExam", p, commandType: CommandType.StoredProcedure);
            }
            catch (SqlException ex) when (ex.Message.Contains("Student has not answered"))
            {
                grade = 0; // No answers submitted — grade is 0
            }

            // Update the grade in Student_Exam after correction
            await conn.ExecuteAsync(
                "UPDATE Student_Exam SET Grade = @Grade WHERE St_ID = @StId AND Exam_ID = @ExamId",
                new { Grade = grade, StId = studentId, ExamId = examId });

            // Get exam metadata for the result page
            const string metaSql = @"
                SELECT e.Total_Degree, c.Course_Name
                FROM Exam e
                INNER JOIN Course c ON e.Course_ID = c.Course_ID
                WHERE e.Exam_ID = @ExamId";

            var meta = await conn.QueryFirstOrDefaultAsync(metaSql, new { ExamId = examId });

            return new ExamResultViewModel
            {
                ExamId = examId,
                CourseName = meta?.Course_Name ?? "",
                Grade = grade,
                TotalDegree = meta?.Total_Degree ?? 100
            };
        }
    }
}
