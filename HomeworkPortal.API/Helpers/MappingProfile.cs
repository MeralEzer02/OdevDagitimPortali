using AutoMapper;
using HomeworkPortal.API.DTOs;
using HomeworkPortal.API.Models;

namespace HomeworkPortal.API.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<AppUser, UserReadDto>();

            // Kullanıcı Kayıt Mapping
            CreateMap<RegisterDto, AppUser>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email));

            // Ders (Course) Mapping
            CreateMap<Course, CourseReadDto>()
                .ForMember(dest => dest.TeacherFullName, opt => opt.MapFrom(src => src.Teacher.FullName));
            CreateMap<CourseCreateDto, Course>();
            CreateMap<CourseUpdateDto, Course>();

            // Ödev (Assignment) Mapping
            CreateMap<Assignment, AssignmentReadDto>()
                .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Course.Name));
            CreateMap<AssignmentCreateDto, Assignment>();
            CreateMap<AssignmentUpdateDto, Assignment>();

            // Teslimat (Submission) Mapping
            CreateMap<Submission, SubmissionReadDto>()
                .ForMember(dest => dest.AssignmentTitle, opt => opt.MapFrom(src => src.Assignment.Title))
                .ForMember(dest => dest.StudentFullName, opt => opt.MapFrom(src => src.Student.FullName));
            CreateMap<SubmissionCreateDto, Submission>();
            CreateMap<SubmissionGradeDto, Submission>();

            // Bildirim (Notification) Mapping
            CreateMap<Notification, NotificationReadDto>();
            CreateMap<NotificationUpdateDto, Notification>();
        }
    }
}