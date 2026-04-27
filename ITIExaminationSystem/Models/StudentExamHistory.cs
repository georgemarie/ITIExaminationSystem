namespace ITIExaminationSystem.Models
{
    /// <summary>
    /// Represents a student's completed exam record shown in exam history.
    /// </summary>
    public class StudentExamHistory
    {
        public int Course_ID { get; set; }
        public string Course_Name { get; set; } = string.Empty;
        public string Exam_Name { get; set; } = string.Empty;
        public decimal? Grade { get; set; }
        public decimal Total_Degree { get; set; }

        /// <summary>Calculated percentage score for display.</summary>
        public decimal Percentage => Total_Degree > 0 ? Math.Round((Grade ?? 0) / Total_Degree * 100, 1) : 0;

        /// <summary>True if student scored 50% or above.</summary>
        public bool IsPassed => Percentage >= 50;
    }
}
