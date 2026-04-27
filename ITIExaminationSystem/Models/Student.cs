namespace ITIExaminationSystem.Models
{
    /// <summary>
    /// Represents a student in the system.
    /// </summary>
    public class Student
    {
        public int St_ID { get; set; }
        public string St_Fname { get; set; } = string.Empty;
        public string St_Lname { get; set; } = string.Empty;
        public string St_Email { get; set; } = string.Empty;
        public string St_Password { get; set; } = string.Empty;
    }
}
