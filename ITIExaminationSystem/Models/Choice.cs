namespace ITIExaminationSystem.Models
{
    /// <summary>
    /// Represents one answer choice for an MCQ question.
    /// </summary>
    public class Choice
    {
        public int Choice_ID { get; set; }
        public int Q_ID { get; set; }
        public string Choice_Text { get; set; } = string.Empty;
        public string Choice_Code { get; set; } = string.Empty; // A, B, C, D / True, False
    }
}
