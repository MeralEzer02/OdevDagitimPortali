using System.Linq.Expressions;

namespace ÖdevDağıtım.API.Models
{
    public class Course : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string TeacherId { get; set; }
        public AppUser Teacher { get; set; }
        public ICollection<Assignment> Assignments { get; set; }
        public ICollection<AppUser> Students { get; set; } = new List<AppUser>();
    }
}
