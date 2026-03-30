using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ÖdevDağıtım.API.DTOs;
using ÖdevDağıtım.API.Models;
using ÖdevDağıtım.API.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ÖdevDağıtım.API.Services
{
    public class SubmissionService : ISubmissionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly INotificationService _notificationService;
        private readonly IFileService _fileService;
        private readonly ILogger<SubmissionService> _logger;

        public SubmissionService(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService, INotificationService notificationService, IFileService fileService, ILogger<SubmissionService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _notificationService = notificationService;
            _fileService = fileService;
            _logger = logger;
        }

        public async Task<SubmissionReadDto> SubmitAssignmentAsync(SubmissionCreateDto dto)
        {
            var studentId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();

            if (dto.AssignmentId <= 0)
                throw new Exception($"Geçersiz Ödev ID'si ({dto.AssignmentId}).");

            var assignment = await _unitOfWork.Assignments.Where(a => a.Id == dto.AssignmentId, a => a.Course).FirstOrDefaultAsync();
            if (assignment == null)
                throw new Exception("Ödev bulunamadı.");

            if (assignment.DueDate < DateTime.UtcNow)
                throw new Exception("Bu ödevin teslim süresi dolmuştur.");

            var courseWithStudents = await _unitOfWork.Courses.Where(c => c.Id == assignment.CourseId, c => c.Students).FirstOrDefaultAsync();
            if (courseWithStudents == null || !courseWithStudents.Students.Any(s => s.Id == studentId))
                throw new Exception("Sadece kayıtlı olduğunuz derslere ödev teslim edebilirsiniz.");

            var submission = _mapper.Map<Submission>(dto);
            submission.StudentId = studentId;
            submission.SubmissionDate = DateTime.UtcNow;

            if (dto.File != null)
            {
                submission.FilePath = await _fileService.UploadFileAsync(dto.File, "submissions");
            }

            try
            {
                await _unitOfWork.Submissions.AddAsync(submission);
                await _unitOfWork.CompleteAsync();
            }
            catch (DbUpdateException ex)
            {
                if (!string.IsNullOrEmpty(submission.FilePath))
                {
                    _fileService.DeleteFile(submission.FilePath);
                }

                _logger.LogWarning("⚠️ DİKKAT: Öğrenci {StudentId}, Ödev {AssignmentId} için mükerrer gönderim yapmaya çalıştı!", studentId, dto.AssignmentId);

                throw new Exception("Bu ödev için zaten bir teslimat yaptınız. (Çoklu gönderim engellendi.)", ex);
            }

            _logger.LogInformation("✅ BAŞARILI: Öğrenci {StudentId}, Ödev {AssignmentId} için dosya yükledi.", studentId, dto.AssignmentId);

            return _mapper.Map<SubmissionReadDto>(submission);
        }

        public async Task GradeSubmissionAsync(int id, SubmissionGradeDto dto)
        {
            var submission = await _unitOfWork.Submissions.Where(s => s.Id == id, s => s.Assignment, s => s.Assignment.Course).FirstOrDefaultAsync();

            if (submission == null) throw new Exception("Teslimat bulunamadı.");

            if (submission.Assignment.Course.TeacherId != _currentUserService.UserId)
                throw new UnauthorizedAccessException("Sadece kendi dersinize ait ödevleri notlandırabilirsiniz.");

            if (dto.RowVersion != null && dto.RowVersion.Length > 0)
            {
                if (submission.RowVersion == null || !submission.RowVersion.SequenceEqual(dto.RowVersion))
                {
                    throw new Exception("Bu ödev notu siz işlem yaparken başka bir öğretmen/sekme tarafından güncellenmiş. Lütfen sayfayı yenileyip tekrar deneyin.");
                }
            }

            submission.Grade = dto.Grade;
            submission.Feedback = dto.Feedback;
            submission.IsGraded = true;

            try
            {
                await _unitOfWork.CompleteAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning("⚠️ DİKKAT: Ödev notlandırma sırasında Concurrency Çatışması yaşandı! Submission ID: {SubmissionId}", id);
                throw new Exception("Bu ödev notu siz işlem yaparken başka bir öğretmen/sekme tarafından güncellenmiş. Lütfen sayfayı yenileyip tekrar deneyin.", ex);
            }

            try
            {
                await _notificationService.CreateNotificationAsync(
                    submission.StudentId,
                    $"{submission.Assignment.Course.Name} dersindeki '{submission.Assignment.Title}' ödeviniz notlandırıldı. Notunuz: {dto.Grade}"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ HATA YUTULDU: Not verildi ama {StudentId} ID'li öğrenciye bildirim atılamadı!", submission.StudentId);
            }
        }

        public async Task<PagedResult<SubmissionReadDto>> GetSubmissionsByAssignmentAsync(int assignmentId, PaginationParams paginationParams)
        {
            var query = _unitOfWork.Submissions.Where(s => s.AssignmentId == assignmentId && !s.IsDeleted);
            var totalCount = await query.CountAsync();

            var submissions = await query
                .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize)
                .ToListAsync();

            var dtoList = _mapper.Map<IEnumerable<SubmissionReadDto>>(submissions);
            return new PagedResult<SubmissionReadDto>(dtoList, totalCount, paginationParams.PageNumber, paginationParams.PageSize);
        }

        public async Task<PagedResult<SubmissionReadDto>> GetStudentSubmissionsAsync(string studentId, PaginationParams paginationParams)
        {
            var query = _unitOfWork.Submissions.Where(s => s.StudentId == studentId && !s.IsDeleted);
            var totalCount = await query.CountAsync();

            var submissions = await query
                .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize)
                .ToListAsync();

            var dtoList = _mapper.Map<IEnumerable<SubmissionReadDto>>(submissions);
            return new PagedResult<SubmissionReadDto>(dtoList, totalCount, paginationParams.PageNumber, paginationParams.PageSize);
        }
    }
}