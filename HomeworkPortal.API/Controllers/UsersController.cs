using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HomeworkPortal.API.Models;
using HomeworkPortal.API.Data;
using HomeworkPortal.API.DTOs;

namespace HomeworkPortal.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Teacher")]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _context;

        public UsersController(UserManager<AppUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            var userList = new List<object>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                var enrolled = await _context.Courses
                    .Where(c => c.Students.Any(s => s.Id == user.Id))
                    .Select(c => c.Name).ToListAsync();

                var created = await _context.Courses
                    .Where(c => c.TeacherId == user.Id)
                    .Select(c => c.Name).ToListAsync();

                userList.Add(new
                {
                    user.Id,
                    user.FullName,
                    user.Email,
                    Roles = roles,
                    EnrolledCourseNames = enrolled,
                    CreatedCourseNames = created
                });
            }
            return Ok(userList);
        }

        // KULLANICI İSİM/SOYİSİM GÜNCELLEME
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateProfileDto dto)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound(new { message = "Kullanıcı bulunamadı." });

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            await _userManager.UpdateAsync(user);

            return Ok(new { message = "Kullanıcı başarıyla güncellendi." });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            await _userManager.DeleteAsync(user);
            return Ok(new { message = "Kullanıcı başarıyla silindi." });
        }
    }
}