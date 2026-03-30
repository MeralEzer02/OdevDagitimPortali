using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ÖdevDağıtım.API.DTOs;
using ÖdevDağıtım.API.Services;

namespace ÖdevDağıtım.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SubmissionsController : ControllerBase
    {
        private readonly ISubmissionService _submissionService;
        private readonly ICurrentUserService _currentUserService;

        public SubmissionsController(ISubmissionService submissionService, ICurrentUserService currentUserService)
        {
            _submissionService = submissionService;
            _currentUserService = currentUserService;
        }

        [HttpPost]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> SubmitAssignment([FromForm] SubmissionCreateDto dto)
        {
            var result = await _submissionService.SubmitAssignmentAsync(dto);
            return Ok(result);
        }

        [HttpGet("assignment/{assignmentId}")]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> GetSubmissionsByAssignment(int assignmentId, [FromQuery] PaginationParams paginationParams)
        {
            var result = await _submissionService.GetSubmissionsByAssignmentAsync(assignmentId, paginationParams);
            return Ok(result);
        }

        [HttpGet("student")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetStudentSubmissions([FromQuery] PaginationParams paginationParams)
        {
            var studentId = _currentUserService.UserId;
            var result = await _submissionService.GetStudentSubmissionsAsync(studentId!, paginationParams);
            return Ok(result);
        }

        [HttpPost("{id}/grade")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> GradeSubmission(int id, [FromBody] SubmissionGradeDto dto)
        {
            await _submissionService.GradeSubmissionAsync(id, dto);
            return Ok(new { message = "Ödev başarıyla notlandırıldı." });
        }
    }
}