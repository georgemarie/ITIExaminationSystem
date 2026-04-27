namespace ITIExaminationSystem.ViewModels
{
    /// <summary>
    /// ViewModel for the exam result page after submission.
    /// </summary>
    public class ExamResultViewModel
    {
        public int ExamId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public decimal Grade { get; set; }
        public decimal TotalDegree { get; set; }
        public decimal Percentage => TotalDegree > 0 ? Math.Round(Grade / TotalDegree * 100, 1) : 0;
        public bool IsPassed => Percentage >= 50;
        public string StatusMessage => IsPassed ? "Congratulations! You Passed! 🎉" : "Keep Practicing! 💪";
        public string StatusClass => IsPassed ? "success" : "danger";
    }
}
