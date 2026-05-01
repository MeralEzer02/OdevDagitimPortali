using HomeworkPortal.API.DTOs;

namespace HomeworkPortal.API.Services
{
    public interface ICourseService
    {
        Task<CourseReadDto> CreateCourseAsync(CourseCreateDto dto);
        Task UpdateCourseAsync(int id, CourseUpdateDto dto);
        Task DeleteCourseAsync(int id);
        Task<PagedResult<CourseReadDto>> GetCoursesAsync(PaginationParams paginationParams);
        Task<CourseReadDto> GetCourseDetailsAsync(int id);
        Task EnrollStudentAsync(int courseId, string studentId);
        Task AssignTeacherAsync(int courseId, string teacherId);

        Task<IEnumerable<UserReadDto>> GetEnrolledStudentsAsync(int courseId);
    }
}