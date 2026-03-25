using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ÖdevDağıtım.API.DTOs;
using ÖdevDağıtım.API.Models;
using ÖdevDağıtım.API.Repositories;

namespace ÖdevDağıtım.API.Services
{
    public class SubmissionService : ISubmissionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly INotificationService _notificationService;

        public SubmissionService(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService, INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _notificationService = notificationService;
        }

        public async Task<SubmissionReadDto> SubmitAssignmentAsync(SubmissionCreateDto dto)
        {
            var studentId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();

            // 🕵️‍♂️ DEDEKTİF KODU 1: Gelen ID'yi kontrol edelim
            if (dto.AssignmentId <= 0)
                throw new Exception($"DEDEKTİF BİLDİRİYOR: JSON'dan gelen AssignmentId değeri sıfır ({dto.AssignmentId}). JSON verisi DTO'ya eşleşemedi! SubmissionCreateDto içindeki yazımı kontrol et.");

            // 🕵️‍♂️ DEDEKTİF KODU 2: Veritabanında bu ID'ye sahip ödev var mı?
            var checkAssignment = await _unitOfWork.Assignments.GetByIdAsync(dto.AssignmentId);
            if (checkAssignment == null)
                throw new Exception($"DEDEKTİF BİLDİRİYOR: Veritabanında Id'si {dto.AssignmentId} olan bir ödev GERÇEKTEN YOK! (Silinmiş veya kaydedilmemiş olabilir).");

            // 🕵️‍♂️ DEDEKTİF KODU 3: İlişkilerle (Course) birlikte çekebiliyor muyuz?
            var assignment = await _unitOfWork.Assignments.Where(a => a.Id == dto.AssignmentId, a => a.Course).FirstOrDefaultAsync();
            if (assignment == null)
                throw new Exception($"DEDEKTİF BİLDİRİYOR: Ödev ({dto.AssignmentId}) bulundu AMA bağlı olduğu Course (Ders) bulunamadı! Veritabanı ilişkisi kopuk.");

            // --- ORİJİNAL KONTROLLER ---
            if (assignment.DueDate < DateTime.UtcNow)
                throw new Exception("Bu ödevin teslim süresi dolmuştur.");

            var courseWithStudents = await _unitOfWork.Courses.Where(c => c.Id == assignment.CourseId, c => c.Students).FirstOrDefaultAsync();
            if (courseWithStudents == null || !courseWithStudents.Students.Any(s => s.Id == studentId))
                throw new Exception("Sadece kayıtlı olduğunuz derslere ödev teslim edebilirsiniz.");

            bool alreadySubmitted = await _unitOfWork.Submissions.Where(s => s.AssignmentId == dto.AssignmentId && s.StudentId == studentId).AnyAsync();
            if (alreadySubmitted)
                throw new Exception("Bu ödev için zaten bir teslimat yaptınız.");

            var submission = _mapper.Map<Submission>(dto);
            submission.StudentId = studentId;
            submission.SubmissionDate = DateTime.UtcNow;

            await _unitOfWork.Submissions.AddAsync(submission);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<SubmissionReadDto>(submission);
        }

        public async Task GradeSubmissionAsync(int id, SubmissionGradeDto dto)
        {
            var submission = await _unitOfWork.Submissions.Where(s => s.Id == id, s => s.Assignment, s => s.Assignment.Course).FirstOrDefaultAsync();

            if (submission == null) throw new Exception("Teslimat bulunamadı.");

            if (submission.Assignment.Course.TeacherId != _currentUserService.UserId)
                throw new UnauthorizedAccessException("Sadece kendi dersinize ait ödevleri notlandırabilirsiniz.");

            submission.Grade = dto.Grade;
            submission.Feedback = dto.Feedback;
            submission.IsGraded = true;

            await _unitOfWork.CompleteAsync();

            await _notificationService.CreateNotificationAsync(
                submission.StudentId,
                $"{submission.Assignment.Course.Name} dersindeki '{submission.Assignment.Title}' ödeviniz notlandırıldı. Notunuz: {dto.Grade}"
            );
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