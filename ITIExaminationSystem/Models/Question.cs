namespace ITIExaminationSystem.Models
{
    /// <summary>
    /// Represents a single exam question (MCQ or True/False).
    /// </summary>
    public class Question
    {
        public int Q_ID { get; set; }
        public string Q_Text { get; set; } = string.Empty;
        public string Q_Type { get; set; } = "MCQ"; // "MCQ" or "T/F"

        // Navigation: choices are loaded separately for each question
        public List<Choice> Choices { get; set; } = new List<Choice>();
    }
}
