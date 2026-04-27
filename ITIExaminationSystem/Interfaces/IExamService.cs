using ITIExaminationSystem.Models;
using ITIExaminationSystem.ViewModels;

namespace ITIExaminationSystem.Interfaces
{
    /// <summary>
    /// Defines all exam-related operations.
    /// Using an interface decouples the controller from the data layer (clean architecture).
    /// </summary>
    public interface IExamService
    {
        /// <summary>Generates a new exam for the given course and returns its ID.</summary>
        Task<int> GenerateExamAsync(int courseId);

        /// <summary>Marks the exam as open and enrolls the student.</summary>
        Task EnrollStudentAsync(int studentId, int examId);

        /// <summary>Loads exam questions (randomized) with their choices.</summary>
        Task<TakeExamViewModel> GetExamForStudentAsync(int examId);

        /// <summary>
        /// Checks if the student has already submitted this exam.
        /// Returns true if already submitted (prevents re-taking).
        /// </summary>
        Task<bool> HasStudentSubmittedAsync(int studentId, int examId);

        /// <summary>Saves one answer for a given question during submission.</summary>
        Task SaveAnswerAsync(int studentId, int examId, int questionId, string answer);

        /// <summary>Calls the correction stored procedure and returns the final grade.</summary>
        Task<ExamResultViewModel> CorrectExamAsync(int studentId, int examId);
    }
}
