using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ÖdevDağıtım.API.DTOs;
using ÖdevDağıtım.API.Services;

namespace ÖdevDağıtım.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class CoursesController : ControllerBase
    {
        private readonly ICourseService _courseService;
        private readonly ICurrentUserService _currentUserService;

        public CoursesController(ICourseService courseService, ICurrentUserService currentUserService)
        {
            _courseService = courseService;
            _currentUserService = currentUserService;
        }

        [HttpPost]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> CreateCourse([FromBody] CourseCreateDto dto)
        {
            var result = await _courseService.CreateCourseAsync(dto);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetCourses([FromQuery] PaginationParams paginationParams)
        {
            var result = await _courseService.GetCoursesAsync(paginationParams);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCourseDetails(int id)
        {
            var result = await _courseService.GetCourseDetailsAsync(id);
            return Ok(result);
        }

        [HttpPost("{id}/enroll")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> EnrollStudent(int id)
        {
            var studentId = _currentUserService.UserId;
            if (studentId == null) return Unauthorized();

            await _courseService.EnrollStudentAsync(id, studentId);
            return Ok(new { message = "Derse başarıyla kayıt oldunuz." });
        }

        [HttpPost("{id}/assign-teacher")]
        [Authorize(Roles = "Admin")] 
        public async Task<IActionResult> AssignTeacher(int id, [FromBody] string teacherId)
        {
            await _courseService.AssignTeacherAsync(id, teacherId);
            return Ok(new { message = "Öğretmen ataması başarılı." });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            await _courseService.DeleteCourseAsync(id);
            return Ok(new { message = "Ders başarıyla silindi." });
        }
    }
}