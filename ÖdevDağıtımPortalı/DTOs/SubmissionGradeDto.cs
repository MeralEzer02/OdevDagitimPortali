namespace ÖdevDağıtım.API.DTOs
{
    public class SubmissionGradeDto
    {
        public int Id { get; set; }
        public double Grade { get; set; }
        public byte[]? RowVersion { get; set; }
        public string Feedback { get; set; } = string.Empty;
    }
}