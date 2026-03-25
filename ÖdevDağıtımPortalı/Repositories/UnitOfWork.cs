using ÖdevDağıtım.API.Data;
using ÖdevDağıtım.API.Models;

namespace ÖdevDağıtım.API.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public ICourseRepository Courses { get; }
        public IAssignmentRepository Assignments { get; }
        public ISubmissionRepository Submissions { get; }
        public IGenericRepository<Notification> Notifications { get; }

        public UnitOfWork(AppDbContext context,
                          ICourseRepository courseRepository,
                          IAssignmentRepository assignmentRepository,
                          ISubmissionRepository submissionRepository,
                          IGenericRepository<Notification> notificationRepository)
        {
            _context = context;
            Courses = courseRepository;
            Assignments = assignmentRepository;
            Submissions = submissionRepository;
            Notifications = notificationRepository;
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}