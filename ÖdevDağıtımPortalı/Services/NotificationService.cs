using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ÖdevDağıtım.API.DTOs;
using ÖdevDağıtım.API.Models;
using ÖdevDağıtım.API.Repositories;

namespace ÖdevDağıtım.API.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public NotificationService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task CreateNotificationAsync(string userId, string message)
        {
            var notification = new Notification
            {
                UserId = userId,
                Message = message,
                IsRead = false
            };

            await _unitOfWork.Notifications.AddAsync(notification);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<PagedResult<NotificationReadDto>> GetUserNotificationsAsync(string userId, PaginationParams paginationParams)
        {
            var query = _unitOfWork.Notifications
                .Where(n => n.UserId == userId && !n.IsDeleted)
                .OrderByDescending(n => n.Created);

            var totalCount = await query.CountAsync();

            var notifications = await query
                .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize)
                .ToListAsync();

            var dtoList = _mapper.Map<IEnumerable<NotificationReadDto>>(notifications);
            return new PagedResult<NotificationReadDto>(dtoList, totalCount, paginationParams.PageNumber, paginationParams.PageSize);
        }

        public async Task MarkAsReadAsync(int id)
        {
            var notification = await _unitOfWork.Notifications.GetByIdAsync(id);
            if (notification != null)
            {
                notification.IsRead = true;
                await _unitOfWork.CompleteAsync();
            }
        }
    }
}