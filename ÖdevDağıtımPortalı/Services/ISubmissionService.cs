using ÖdevDağıtım.API.DTOs;

namespace ÖdevDağıtım.API.Services
{
    public interface ISubmissionService
    {
        Task<SubmissionReadDto> SubmitAssignmentAsync(SubmissionCreateDto dto);
        Task<PagedResult<SubmissionReadDto>> GetSubmissionsByAssignmentAsync(int assignmentId, PaginationParams paginationParams);
        Task<PagedResult<SubmissionReadDto>> GetStudentSubmissionsAsync(string studentId, PaginationParams paginationParams);
        Task GradeSubmissionAsync(int id, SubmissionGradeDto dto);
    }
}