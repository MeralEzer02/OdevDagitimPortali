using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using HomeworkPortal.API.DTOs;
using HomeworkPortal.API.Models;
using HomeworkPortal.API.Repositories;

namespace HomeworkPortal.API.Services
{
    public class CourseService : ICourseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly UserManager<AppUser> _userManager;
        private readonly INotificationService _notificationService;

        public CourseService(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService, UserManager<AppUser> userManager, INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        public async Task<CourseReadDto> CreateCourseAsync(CourseCreateDto dto)
        {
            var newCourse = _mapper.Map<Course>(dto);
            newCourse.TeacherId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();

            await _unitOfWork.Courses.AddAsync(newCourse);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<CourseReadDto>(newCourse);
        }

        public async Task UpdateCourseAsync(int id, CourseUpdateDto dto)
        {
            var course = await _unitOfWork.Courses.GetByIdAsync(id);
            if (course == null) throw new Exception("Ders bulunamadı.");

            // 👑 ADMIN KONTROLÜ
            var user = await _userManager.FindByIdAsync(_currentUserService.UserId);
            var isAdmin = user != null && await _userManager.IsInRoleAsync(user, "Admin");

            if (course.TeacherId != _currentUserService.UserId && !isAdmin)
                throw new UnauthorizedAccessException("Sadece kendi açtığınız dersi güncelleyebilirsiniz.");

            _mapper.Map(dto, course);
            await _unitOfWork.CompleteAsync();
        }

        public async Task DeleteCourseAsync(int id)
        {
            var course = await _unitOfWork.Courses.GetByIdAsync(id);
            if (course == null) throw new Exception("Ders bulunamadı.");

            // 👑 ADMIN KONTROLÜ
            var user = await _userManager.FindByIdAsync(_currentUserService.UserId);
            var isAdmin = user != null && await _userManager.IsInRoleAsync(user, "Admin");

            if (course.TeacherId != _currentUserService.UserId && !isAdmin)
                throw new UnauthorizedAccessException("Sadece kendi açtığınız dersi silebilirsiniz.");

            course.IsDeleted = true;
            await _unitOfWork.CompleteAsync();
        }

        public async Task<PagedResult<CourseReadDto>> GetCoursesAsync(PaginationParams paginationParams)
        {
            var userId = _currentUserService.UserId;
            var user = await _userManager.FindByIdAsync(userId);
            var isAdmin = user != null && await _userManager.IsInRoleAsync(user, "Admin");
            var isTeacher = user != null && await _userManager.IsInRoleAsync(user, "Teacher");

            var query = _unitOfWork.Courses.GetAll(c => c.Teacher).AsQueryable();

            // 🚀 İŞTE DÜZELTİLEN YER: Eğer öğretmense ve Admin değilse, SADECE kendi derslerini görecek!
            if (isTeacher && !isAdmin)
            {
                query = query.Where(c => c.TeacherId == userId);
            }
            // Not: Öğrenciler derslere "Kayıt Ol" diyebilsinler diye onlara tüm listeyi açık bırakıyoruz.

            var totalCount = await query.CountAsync();

            var courses = await query
                .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize)
                .ToListAsync();

            var dtoList = _mapper.Map<IEnumerable<CourseReadDto>>(courses);

            return new PagedResult<CourseReadDto>(dtoList, totalCount, paginationParams.PageNumber, paginationParams.PageSize);
        }

        public async Task<CourseReadDto> GetCourseDetailsAsync(int id)
        {
            var course = await _unitOfWork.Courses.Where(c => c.Id == id, c => c.Teacher).FirstOrDefaultAsync();
            return _mapper.Map<CourseReadDto>(course);
        }

        public async Task EnrollStudentAsync(int courseId, string studentId)
        {
            var course = await _unitOfWork.Courses.Where(c => c.Id == courseId, c => c.Students).FirstOrDefaultAsync();
            if (course == null) throw new Exception("Ders bulunamadı.");

            var student = await _userManager.FindByIdAsync(studentId);
            if (student == null) throw new Exception("Öğrenci bulunamadı.");

            if (course.Students.Any(s => s.Id == studentId))
                throw new Exception("Bu derse zaten kayıtlısınız.");

            course.Students.Add(student);

            await _unitOfWork.CompleteAsync();

            try
            {
                await _notificationService.CreateNotificationAsync(
                    course.TeacherId,
                    $"{student.FirstName} {student.LastName} isimli öğrenci {course.Name} dersinize kayıt oldu."
                );
            }
            catch (Exception)
            {
            }
        }

        public async Task AssignTeacherAsync(int courseId, string teacherId)
        {
            var course = await _unitOfWork.Courses.GetByIdAsync(courseId);
            if (course == null) throw new Exception("Ders bulunamadı.");

            course.TeacherId = teacherId;
            await _unitOfWork.CompleteAsync();
        }

        public async Task<IEnumerable<UserReadDto>> GetEnrolledStudentsAsync(int courseId)
        {
            var course = await _unitOfWork.Courses
                .Where(c => c.Id == courseId, c => c.Students)
                .FirstOrDefaultAsync();

            if (course == null)
                throw new Exception("Ders bulunamadı.");

            return _mapper.Map<IEnumerable<UserReadDto>>(course.Students);
        }
    }
}