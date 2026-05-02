namespace HomeworkPortal.API.DTOs
{
    public class StudentEnrollmentDto
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string StudentFullName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
    }
}