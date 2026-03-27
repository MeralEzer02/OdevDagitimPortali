using Microsoft.EntityFrameworkCore;

namespace ÖdevDağıtım.API.Models
{
    [Index(nameof(AssignmentId), nameof(StudentId), IsUnique = true)]
    public class Submission : BaseEntity
    {
        public DateTime SubmissionDate { get; set; }
        public string? FilePath { get; set; }
        public int AssignmentId { get; set; }
        public Assignment Assignment { get; set; }
        public string StudentId { get; set; }
        public AppUser Student { get; set; }
        public bool IsGraded { get; set; } = false;
        public double? Grade { get; set; }
        public string? Feedback { get; set; }
    }
}