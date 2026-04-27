using ITIExaminationSystem.Models;

namespace ITIExaminationSystem.ViewModels
{
    /// <summary>
    /// ViewModel for the student dashboard page.
    /// Separates view data from domain models (clean architecture).
    /// </summary>
    public class StudentDashboardViewModel
    {
        public string StudentName { get; set; } = string.Empty;
        public List<Course> AvailableCourses { get; set; } = new();
        public List<StudentExamHistory> ExamHistory { get; set; } = new();

        // Summary stats for the dashboard cards
        public int TotalExamsTaken => ExamHistory.Count;
        public int TotalPassed => ExamHistory.Count(h => h.IsPassed);
        public decimal AverageScore => ExamHistory.Any()
            ? Math.Round(ExamHistory.Average(h => h.Percentage), 1)
            : 0;
    }
}
