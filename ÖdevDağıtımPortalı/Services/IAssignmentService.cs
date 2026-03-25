using ÖdevDağıtım.API.DTOs;

namespace ÖdevDağıtım.API.Services
{
    public interface IAssignmentService
    {
        Task<AssignmentReadDto> CreateAssignmentAsync(AssignmentCreateDto dto);
        Task UpdateAssignmentAsync(int id, AssignmentUpdateDto dto);
        Task DeleteAssignmentAsync(int id);
        Task<PagedResult<AssignmentReadDto>> GetAssignmentsByCourseAsync(int courseId, PaginationParams paginationParams);
        Task<PagedResult<AssignmentReadDto>> GetAssignmentsForStudentAsync(string studentId, PaginationParams paginationParams);
        Task<AssignmentReadDto> GetAssignmentDetailsAsync(int id);
    }
}