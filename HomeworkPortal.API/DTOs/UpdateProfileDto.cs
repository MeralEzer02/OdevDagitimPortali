namespace HomeworkPortal.API.DTOs
{
    public class UpdateProfileDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? NewPassword { get; set; }
    }
}