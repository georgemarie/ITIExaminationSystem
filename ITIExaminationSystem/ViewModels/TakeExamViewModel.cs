using ITIExaminationSystem.Models;

namespace ITIExaminationSystem.ViewModels
{
    /// <summary>
    /// ViewModel for the active exam page.
    /// Includes exam metadata needed for the timer and submission.
    /// </summary>
    public class TakeExamViewModel
    {
        public int ExamId { get; set; }
        public string ExamName { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;

        /// <summary>Duration in minutes for the countdown timer.</summary>
        public int DurationMinutes { get; set; } = 30;

        /// <summary>UTC time when the student started this exam attempt.</summary>
        public DateTime StartedAt { get; set; }

        /// <summary>Randomized list of questions for this attempt.</summary>
        public List<Question> Questions { get; set; } = new();
    }
}
