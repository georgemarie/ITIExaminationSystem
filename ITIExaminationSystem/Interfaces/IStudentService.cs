using ITIExaminationSystem.Models;
using ITIExaminationSystem.ViewModels;

namespace ITIExaminationSystem.Interfaces
{
    /// <summary>
    /// Defines all student-related data operations.
    /// </summary>
    public interface IStudentService
    {
        /// <summary>Authenticates a user by email and password. Returns Student ID or null.</summary>
        Task<int?> AuthenticateAsync(string email, string password);

        /// <summary>Loads the full student dashboard data (profile + courses + history).</summary>
        Task<StudentDashboardViewModel> GetDashboardAsync(int studentId);

        /// <summary>Gets the student's first name for display.</summary>
        Task<string> GetStudentNameAsync(int studentId);
    }
}
