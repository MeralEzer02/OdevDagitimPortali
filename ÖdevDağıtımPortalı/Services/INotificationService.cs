using ÖdevDağıtım.API.DTOs;

namespace ÖdevDağıtım.API.Services
{
    public interface INotificationService
    {
        Task CreateNotificationAsync(string userId, string message);
        Task<PagedResult<NotificationReadDto>> GetUserNotificationsAsync(string userId, PaginationParams paginationParams);
        Task MarkAsReadAsync(int id);
    }
}