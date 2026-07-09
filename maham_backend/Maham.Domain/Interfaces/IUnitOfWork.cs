using System;
using System.Threading.Tasks;
using Maham.Domain.Entities;

namespace Maham.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<User> Users { get; }
    IRepository<Project> Projects { get; }
    IRepository<Board> Boards { get; }
    IRepository<Column> Columns { get; }
    IRepository<Card> Cards { get; }
    IRepository<Comment> Comments { get; }
    IRepository<Attachment> Attachments { get; }
    IRepository<ActivityLog> ActivityLogs { get; }
    IRepository<ProjectMember> ProjectMembers { get; }
    Task<int> SaveChangesAsync();
}
