using System;
using System.Threading.Tasks;
using Maham.Domain.Entities;
using Maham.Domain.Interfaces;
using Maham.Infrastructure.Data;

namespace Maham.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        Users = new Repository<User>(_context);
        Projects = new Repository<Project>(_context);
        Boards = new Repository<Board>(_context);
        Columns = new Repository<Column>(_context);
        Cards = new Repository<Card>(_context);
        Comments = new Repository<Comment>(_context);
        Attachments = new Repository<Attachment>(_context);
        ActivityLogs = new Repository<ActivityLog>(_context);
        ProjectMembers = new Repository<ProjectMember>(_context);
    }

    public IRepository<User> Users { get; }
    public IRepository<Project> Projects { get; }
    public IRepository<Board> Boards { get; }
    public IRepository<Column> Columns { get; }
    public IRepository<Card> Cards { get; }
    public IRepository<Comment> Comments { get; }
    public IRepository<Attachment> Attachments { get; }
    public IRepository<ActivityLog> ActivityLogs { get; }
    public IRepository<ProjectMember> ProjectMembers { get; }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
