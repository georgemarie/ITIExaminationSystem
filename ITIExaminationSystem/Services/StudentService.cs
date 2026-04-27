using Dapper;
using ITIExaminationSystem.Interfaces;
using ITIExaminationSystem.Models;
using ITIExaminationSystem.ViewModels;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ITIExaminationSystem.Services
{
    /// <summary>
    /// Handles all student-related database operations using Dapper.
    /// Keeps controllers thin — all SQL lives here.
    /// </summary>
    public class StudentService : IStudentService
    {
        private readonly IConfiguration _config;

        public StudentService(IConfiguration config)
        {
            _config = config;
        }

        private SqlConnection GetConnection() =>
            new SqlConnection(_config.GetConnectionString("DefaultConnection"));

        /// <summary>
        /// Authenticates login credentials.
        /// Returns the linked Student ID, or null if login fails.
        /// </summary>
        public async Task<int?> AuthenticateAsync(string email, string password)
        {
            using var conn = GetConnection();

            // Step 1: Verify the user exists in the [User] table
            const string authSql = "SELECT COUNT(1) FROM [User] WHERE Email = @Email AND Password = @Password";
            int count = await conn.ExecuteScalarAsync<int>(authSql, new { Email = email, Password = password });

            if (count == 0)
                return null; // Invalid credentials

            // Step 2: Get the student linked to that email
            const string studentSql = "SELECT St_ID FROM Student WHERE UserEmail = @Email";
            int studentId = await conn.ExecuteScalarAsync<int>(studentSql, new { Email = email });

            return studentId > 0 ? studentId : null;
        }

        /// <summary>
        /// Loads all data needed for the student dashboard.
        /// </summary>
        public async Task<StudentDashboardViewModel> GetDashboardAsync(int studentId)
        {
            using var conn = GetConnection();

            // Get student name
            const string nameSql = "SELECT St_Fname FROM Student WHERE St_ID = @Id";
            string name = await conn.ExecuteScalarAsync<string>(nameSql, new { Id = studentId }) ?? "Student";

            // Get exam history with course/grade details
            const string historySql = @"
                SELECT c.Course_ID, c.Course_Name, e.Exam_Name, se.Grade, e.Total_Degree
                FROM Student_Exam se
                INNER JOIN Exam e ON se.Exam_ID = e.Exam_ID
                INNER JOIN Course c ON e.Course_ID = c.Course_ID
                WHERE se.St_ID = @StId AND se.IsSubmitted = 1
                ORDER BY e.Exam_ID DESC";

            var history = (await conn.QueryAsync<StudentExamHistory>(historySql, new { StId = studentId })).ToList();

            // Get all available courses using existing stored procedure
            var allCourses = (await conn.QueryAsync<Course>("SP_Get_Course", commandType: CommandType.StoredProcedure)).ToList();

            // Filter out courses the student already took
            var takenCourseIds = history.Select(h => h.Course_ID).Distinct().ToHashSet();
            var availableCourses = allCourses.Where(c => !takenCourseIds.Contains(c.Course_ID)).ToList();

            return new StudentDashboardViewModel
            {
                StudentName = name,
                AvailableCourses = availableCourses,
                ExamHistory = history
            };
        }

        public async Task<string> GetStudentNameAsync(int studentId)
        {
            using var conn = GetConnection();
            const string sql = "SELECT St_Fname FROM Student WHERE St_ID = @Id";
            return await conn.ExecuteScalarAsync<string>(sql, new { Id = studentId }) ?? "Student";
        }
    }
}
