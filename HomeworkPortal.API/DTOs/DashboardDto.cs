namespace HomeworkPortal.API.DTOs
{
    // Admin için ana DTO
    public class DashboardDto
    {
        public int TotalStudents { get; set; }
        public int TotalTeachers { get; set; }
        public int TotalCourses { get; set; }
        public int TotalAssignments { get; set; }

        // Admin için yeni eklenen listeler
        public List<AssignmentSummaryDto> ActiveAssignments { get; set; } = new();
        public List<AssignmentSummaryDto> ExpiredAssignments { get; set; } = new();
        public List<CourseSummaryDto> RecentCourses { get; set; } = new();
    }

    // Öğretmen için ana DTO
    public class TeacherDashboardDto
    {
        public int MyTotalStudents { get; set; }
        public int MyTotalCourses { get; set; }
        public int MyTotalAssignments { get; set; }
        public int PendingSubmissions { get; set; }
        public List<AssignmentSummaryDto> ActiveAssignments { get; set; } = new();
        public List<AssignmentSummaryDto> ExpiredAssignments { get; set; } = new();
        public List<CourseSummaryDto> RecentCourses { get; set; } = new();
    }

    // Ödev özet bilgileri
    public class AssignmentSummaryDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
    }

    // Kurs özet bilgileri
    public class CourseSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int EnrolledStudentCount { get; set; }
        public string TeacherFullName { get; set; } = string.Empty;
    }
}