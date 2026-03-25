using ÖdevDağıtım.API.Models;

namespace ÖdevDağıtım.API.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        ICourseRepository Courses { get; }
        IAssignmentRepository Assignments { get; }
        ISubmissionRepository Submissions { get; }

        Task<int> CompleteAsync();
        IGenericRepository<Notification> Notifications { get; }
    }
}