using Microsoft.EntityFrameworkCore;
using Maham.Domain.Entities;
using Maham.Domain.Enums;

namespace Maham.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Project> Projects { get; set; } = null!;
    public DbSet<ProjectMember> ProjectMembers { get; set; } = null!;
    public DbSet<Board> Boards { get; set; } = null!;
    public DbSet<Column> Columns { get; set; } = null!;
    public DbSet<Card> Cards { get; set; } = null!;
    public DbSet<Comment> Comments { get; set; } = null!;
    public DbSet<Attachment> Attachments { get; set; } = null!;
    public DbSet<ActivityLog> ActivityLogs { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. User Configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.AvatarUrl);
            entity.Property(e => e.Role)
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue(UserRole.Member);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
        });

        // 2. Project Configuration
        modelBuilder.Entity<Project>(entity =>
        {
            entity.ToTable("Projects");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");

            entity.HasOne(e => e.Owner)
                .WithMany(u => u.OwnedProjects)
                .HasForeignKey(e => e.OwnerId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent user deletion if they own projects
        });

        // 3. ProjectMember Configuration
        modelBuilder.Entity<ProjectMember>(entity =>
        {
            entity.ToTable("ProjectMembers");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Role)
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue(ProjectMemberRole.Member);
            entity.Property(e => e.JoinedAt).HasDefaultValueSql("NOW()");

            entity.HasIndex(e => new { e.ProjectId, e.UserId }).IsUnique();

            entity.HasOne(e => e.Project)
                .WithMany(p => p.Members)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany(u => u.ProjectMemberships)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // 4. Board Configuration
        modelBuilder.Entity<Board>(entity =>
        {
            entity.ToTable("Boards");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description);
            entity.Property(e => e.BackgroundColor).HasMaxLength(7);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");

            entity.HasOne(e => e.Project)
                .WithMany(p => p.Boards)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // 5. Column Configuration
        modelBuilder.Entity<Column>(entity =>
        {
            entity.ToTable("Columns");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Order).IsRequired();
            entity.Property(e => e.WipLimit);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");

            entity.HasOne(e => e.Board)
                .WithMany(b => b.Columns)
                .HasForeignKey(e => e.BoardId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // 6. Card Configuration
        modelBuilder.Entity<Card>(entity =>
        {
            entity.ToTable("Cards");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(300);
            entity.Property(e => e.Description);
            entity.Property(e => e.Priority)
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue(Priority.Medium);
            entity.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue(CardStatus.Todo);
            entity.Property(e => e.CoverImageUrl);
            entity.Property(e => e.Order).IsRequired();
            entity.Property(e => e.StoryPoints);
            entity.Property(e => e.Labels);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");

            entity.HasOne(e => e.Column)
                .WithMany(c => c.Cards)
                .HasForeignKey(e => e.ColumnId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Assignee)
                .WithMany(u => u.AssignedCards)
                .HasForeignKey(e => e.AssigneeId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // 7. Comment Configuration
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.ToTable("Comments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.IsEdited).HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");

            entity.HasOne(e => e.Card)
                .WithMany(c => c.Comments)
                .HasForeignKey(e => e.CardId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Author)
                .WithMany(u => u.Comments)
                .HasForeignKey(e => e.AuthorId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // 8. Attachment Configuration
        modelBuilder.Entity<Attachment>(entity =>
        {
            entity.ToTable("Attachments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(300);
            entity.Property(e => e.FilePath).IsRequired();
            entity.Property(e => e.FileSize).IsRequired();
            entity.Property(e => e.MimeType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.UploadedAt).HasDefaultValueSql("NOW()");

            entity.HasOne(e => e.Card)
                .WithMany(c => c.Attachments)
                .HasForeignKey(e => e.CardId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.UploadedBy)
                .WithMany(u => u.UploadedAttachments)
                .HasForeignKey(e => e.UploadedById)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // 9. ActivityLog Configuration
        modelBuilder.Entity<ActivityLog>(entity =>
        {
            entity.ToTable("ActivityLogs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EntityType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.Metadata);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");

            entity.HasOne(e => e.Project)
                .WithMany(p => p.ActivityLogs)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany(u => u.ActivityLogs)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
