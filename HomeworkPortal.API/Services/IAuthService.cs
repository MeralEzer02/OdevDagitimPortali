using HomeworkPortal.API.DTOs;

namespace HomeworkPortal.API.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task<bool> AssignRoleAsync(string email, string roleName);
        Task<AuthResponseDto> UpdateProfileAsync(string userId, UpdateProfileDto dto);
    }
}