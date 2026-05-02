using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HomeworkPortal.API.DTOs;
using HomeworkPortal.API.Services;
using HomeworkPortal.API.Data;
using HomeworkPortal.API.Models;

namespace HomeworkPortal.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseService _courseService;
        private readonly ICurrentUserService _currentUserService;
        private readonly AppDbContext _context;

        public CoursesController(ICourseService courseService, ICurrentUserService currentUserService, AppDbContext context)
        {
            _courseService = courseService;
            _currentUserService = currentUserService;
            _context = context;
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

        [HttpPut("{id}")]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> UpdateCourse(int id, [FromBody] CourseUpdateDto dto)
        {
            if (dto.Id == 0) dto.Id = id;
            await _courseService.UpdateCourseAsync(id, dto);
            return Ok(new { message = "Kurs başarıyla güncellendi." });
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

        [HttpGet("{id}/students")]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> GetEnrolledStudents(int id)
        {
            try
            {
                var result = await _courseService.GetEnrolledStudentsAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/assign-student")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignStudentToCourse(int id, [FromQuery] string studentId)
        {
            var course = await _context.Courses.Include(c => c.Students).FirstOrDefaultAsync(c => c.Id == id);
            if (course == null) return NotFound(new { message = "Kurs bulunamadı." });

            var student = await _context.Users.FindAsync(studentId);
            if (student == null) return NotFound(new { message = "Öğrenci bulunamadı." });

            if (!course.Students.Any(s => s.Id == studentId))
            {
                course.Students.Add((AppUser)student);
                await _context.SaveChangesAsync();
            }
            return Ok(new { message = "Öğrenci kursa başarıyla eklendi." });
        }

        [HttpPost("{id}/remove-student")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveStudentFromCourse(int id, [FromQuery] string studentId)
        {
            var course = await _context.Courses.Include(c => c.Students).FirstOrDefaultAsync(c => c.Id == id);
            if (course == null) return NotFound(new { message = "Kurs bulunamadı." });

            var student = course.Students.FirstOrDefault(s => s.Id == studentId);
            if (student != null)
            {
                course.Students.Remove(student);
                await _context.SaveChangesAsync();
            }
            return Ok(new { message = "Öğrenci kurstan başarıyla çıkarıldı." });
        }

        [HttpGet("all-enrolled-students")]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> GetAllEnrolledStudents()
        {
            try
            {
                var courses = await _context.Courses
                    .Include(c => c.Students)
                    .Select(c => new
                    {
                        CourseId = c.Id,
                        CourseName = c.Name,
                        TeacherId = c.TeacherId,
                        Students = c.Students.Select(s => new { s.Id, s.FullName, s.Email }).ToList()
                    })
                    .ToListAsync();

                return Ok(courses);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}