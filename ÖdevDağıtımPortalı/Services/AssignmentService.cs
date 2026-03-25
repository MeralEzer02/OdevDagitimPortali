using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ÖdevDağıtım.API.DTOs;
using ÖdevDağıtım.API.Models;
using ÖdevDağıtım.API.Repositories;

namespace ÖdevDağıtım.API.Services
{
    public class AssignmentService : IAssignmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly INotificationService _notificationService;

        public AssignmentService(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService, INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _notificationService = notificationService;
        }

        public async Task<AssignmentReadDto> CreateAssignmentAsync(AssignmentCreateDto dto)
        {
            if (dto.DueDate <= DateTime.UtcNow)
                throw new Exception("Son teslim tarihi geçmiş bir zaman olamaz.");

            var course = await _unitOfWork.Courses.GetByIdAsync(dto.CourseId);
            if (course == null) throw new Exception("Ders bulunamadı.");

            if (course.TeacherId != _currentUserService.UserId)
                throw new UnauthorizedAccessException("Sadece kendi dersinize ödev ekleyebilirsiniz.");

            var assignment = _mapper.Map<Assignment>(dto);
            await _unitOfWork.Assignments.AddAsync(assignment);
            await _unitOfWork.CompleteAsync();

            var courseWithStudents = await _unitOfWork.Courses
                .Where(c => c.Id == dto.CourseId, c => c.Students)
                .FirstOrDefaultAsync();

            if (courseWithStudents != null && courseWithStudents.Students.Any())
            {
                foreach (var student in courseWithStudents.Students)
                {
                    await _notificationService.CreateNotificationAsync(
                        student.Id,
                        $"{courseWithStudents.Name} dersine yeni bir ödev eklendi: {assignment.Title}. Son Teslim: {assignment.DueDate.ToShortDateString()}"
                    );
                }
            }

            return _mapper.Map<AssignmentReadDto>(assignment);
        }

        public async Task UpdateAssignmentAsync(int id, AssignmentUpdateDto dto)
        {
            if (dto.DueDate <= DateTime.UtcNow)
                throw new Exception("Son teslim tarihi geçmiş bir zaman olamaz.");

            var assignment = await _unitOfWork.Assignments.Where(a => a.Id == id, a => a.Course).FirstOrDefaultAsync();
            if (assignment == null) throw new Exception("Ödev bulunamadı.");

            if (assignment.Course.TeacherId != _currentUserService.UserId)
                throw new UnauthorizedAccessException("Sadece kendi dersinizin ödevini güncelleyebilirsiniz.");

            _mapper.Map(dto, assignment);
            await _unitOfWork.CompleteAsync();
        }

        public async Task DeleteAssignmentAsync(int id)
        {
            var assignment = await _unitOfWork.Assignments.Where(a => a.Id == id, a => a.Course).FirstOrDefaultAsync();
            if (assignment == null) throw new Exception("Ödev bulunamadı.");

            if (assignment.Course.TeacherId != _currentUserService.UserId)
                throw new UnauthorizedAccessException("Sadece kendi dersinizin ödevini silebilirsiniz.");

            assignment.IsDeleted = true;
            await _unitOfWork.CompleteAsync();
        }

        public async Task<PagedResult<AssignmentReadDto>> GetAssignmentsByCourseAsync(int courseId, PaginationParams paginationParams)
        {
            var query = _unitOfWork.Assignments.Where(a => a.CourseId == courseId && !a.IsDeleted);
            var totalCount = await query.CountAsync();

            var assignments = await query
                .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize)
                .ToListAsync();

            var dtoList = _mapper.Map<IEnumerable<AssignmentReadDto>>(assignments);
            return new PagedResult<AssignmentReadDto>(dtoList, totalCount, paginationParams.PageNumber, paginationParams.PageSize);
        }

        public async Task<PagedResult<AssignmentReadDto>> GetAssignmentsForStudentAsync(string studentId, PaginationParams paginationParams)
        {
            var courses = await _unitOfWork.Courses.Where(c => c.Students.Any(s => s.Id == studentId)).Select(c => c.Id).ToListAsync();

            var query = _unitOfWork.Assignments.Where(a => courses.Contains(a.CourseId) && !a.IsDeleted);
            var totalCount = await query.CountAsync();

            var assignments = await query
                .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize)
                .ToListAsync();

            var dtoList = _mapper.Map<IEnumerable<AssignmentReadDto>>(assignments);
            return new PagedResult<AssignmentReadDto>(dtoList, totalCount, paginationParams.PageNumber, paginationParams.PageSize);
        }

        public async Task<AssignmentReadDto> GetAssignmentDetailsAsync(int id)
        {
            var assignment = await _unitOfWork.Assignments.GetByIdAsync(id);
            return _mapper.Map<AssignmentReadDto>(assignment);
        }
    }
}