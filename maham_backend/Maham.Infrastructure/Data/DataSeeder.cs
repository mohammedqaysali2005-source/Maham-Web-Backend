using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Maham.Domain.Entities;
using Maham.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Maham.Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // Ensure Database is Created
        await context.Database.EnsureCreatedAsync();

        // 1. Check if database needs rich demo seeding (if cards count is less than 10)
        var needsReseed = await context.Users.AnyAsync(u => u.Email == "m.qahtan@maham.com" && u.Role == UserRole.Admin);
        if (!needsReseed && await context.Cards.CountAsync() >= 10)
        {
            return;
        }

        // Clear existing data to re-seed cleanly if data is partial
        if (await context.Users.AnyAsync())
        {
            context.ActivityLogs.RemoveRange(context.ActivityLogs);
            context.Comments.RemoveRange(context.Comments);
            context.Attachments.RemoveRange(context.Attachments);
            context.Cards.RemoveRange(context.Cards);
            context.Columns.RemoveRange(context.Columns);
            context.Boards.RemoveRange(context.Boards);
            context.ProjectMembers.RemoveRange(context.ProjectMembers);
            context.Projects.RemoveRange(context.Projects);
            context.Users.RemoveRange(context.Users);
            await context.SaveChangesAsync();
        }

        var defaultPasswordHash = BCrypt.Net.BCrypt.HashPassword("12345678");

        // ──────────────────────────────────────────────
        // 1. USERS (6 Users with distinct roles)
        // ──────────────────────────────────────────────
        var adminUser = new User
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            FullName = "المدير العام (System Admin)",
            Email = "admin@maham.com",
            PasswordHash = defaultPasswordHash,
            Role = UserRole.Admin,
            CreatedAt = DateTime.UtcNow.AddDays(-60)
        };

        var leadUser = new User
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            FullName = "محمد قيس القباطي (Lead Developer)",
            Email = "m.qahtan@maham.com",
            PasswordHash = defaultPasswordHash,
            Role = UserRole.Member,
            CreatedAt = DateTime.UtcNow.AddDays(-55)
        };

        var pmUser = new User
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            FullName = "ريم السقاف (Project Manager)",
            Email = "reem.pm@maham.com",
            PasswordHash = defaultPasswordHash,
            Role = UserRole.Member,
            CreatedAt = DateTime.UtcNow.AddDays(-50)
        };

        var devUser = new User
        {
            Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
            FullName = "سارة خالد (Front-End Developer)",
            Email = "sara.dev@maham.com",
            PasswordHash = defaultPasswordHash,
            Role = UserRole.Member,
            CreatedAt = DateTime.UtcNow.AddDays(-45)
        };

        var qaUser = new User
        {
            Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
            FullName = "أحمد علي (QA Engineer)",
            Email = "ahmed.qa@maham.com",
            PasswordHash = defaultPasswordHash,
            Role = UserRole.Member,
            CreatedAt = DateTime.UtcNow.AddDays(-40)
        };

        var backendDev = new User
        {
            Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
            FullName = "خالد البعداني (Back-End Developer)",
            Email = "khaled.backend@maham.com",
            PasswordHash = defaultPasswordHash,
            Role = UserRole.Member,
            CreatedAt = DateTime.UtcNow.AddDays(-35)
        };

        await context.Users.AddRangeAsync(adminUser, leadUser, pmUser, devUser, qaUser, backendDev);
        await context.SaveChangesAsync();

        // ──────────────────────────────────────────────
        // 2. PROJECTS (4 Real World Projects)
        // ──────────────────────────────────────────────
        var proj1 = new Project
        {
            Id = Guid.Parse("a1111111-1111-1111-1111-111111111111"),
            Name = "منصة مهام لإدارة المشاريع (Maham Web)",
            Description = "تطوير نظام إدارة المهام والمشاريع التفاعلي باستخدام ASP.NET Core MVC و Clean Architecture ونظام الكانبان اللحظي.",
            OwnerId = leadUser.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        var proj2 = new Project
        {
            Id = Guid.Parse("a2222222-2222-2222-2222-222222222222"),
            Name = "تطبيق الهواتف الذكية (Flutter Mobile)",
            Description = "تطوير تطبيق الهاتف المحمول بنظام Flutter ومزامنة البيانات محلياً عبر REST API و SignalR.",
            OwnerId = pmUser.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-25)
        };

        var proj3 = new Project
        {
            Id = Guid.Parse("a3333333-3333-3333-3333-333333333333"),
            Name = "نظام الذكاء الاصطناعي وتتبع الإنتاجية (AI Assistant)",
            Description = "مشروع اختياري لتوليد التوصيات وتلخيص حالة المهام اليومية باستخدام نماذج الذكاء الاصطناعي.",
            OwnerId = leadUser.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-15)
        };

        var proj4 = new Project
        {
            Id = Guid.Parse("a4444444-4444-4444-4444-444444444444"),
            Name = "البنية التحتية والأمان (DevOps & Security)",
            Description = "إعداد الحاويات Docker، وأمن التشفير JWT، وإعداد سيرفرات التشغيل المستمر CI/CD.",
            OwnerId = pmUser.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        };

        await context.Projects.AddRangeAsync(proj1, proj2, proj3, proj4);
        await context.SaveChangesAsync();

        // ──────────────────────────────────────────────
        // 3. PROJECT MEMBERS
        // ──────────────────────────────────────────────
        var members = new List<ProjectMember>
        {
            new() { ProjectId = proj1.Id, UserId = leadUser.Id, Role = ProjectMemberRole.Owner, Status = ProjectMemberStatus.Accepted },
            new() { ProjectId = proj1.Id, UserId = adminUser.Id, Role = ProjectMemberRole.Admin, Status = ProjectMemberStatus.Accepted },
            new() { ProjectId = proj1.Id, UserId = pmUser.Id, Role = ProjectMemberRole.Admin, Status = ProjectMemberStatus.Accepted },
            new() { ProjectId = proj1.Id, UserId = devUser.Id, Role = ProjectMemberRole.Member, Status = ProjectMemberStatus.Accepted },
            new() { ProjectId = proj1.Id, UserId = qaUser.Id, Role = ProjectMemberRole.Member, Status = ProjectMemberStatus.Accepted },
            new() { ProjectId = proj1.Id, UserId = backendDev.Id, Role = ProjectMemberRole.Member, Status = ProjectMemberStatus.Accepted },

            new() { ProjectId = proj2.Id, UserId = pmUser.Id, Role = ProjectMemberRole.Owner, Status = ProjectMemberStatus.Accepted },
            new() { ProjectId = proj2.Id, UserId = adminUser.Id, Role = ProjectMemberRole.Admin, Status = ProjectMemberStatus.Accepted },
            new() { ProjectId = proj2.Id, UserId = leadUser.Id, Role = ProjectMemberRole.Admin, Status = ProjectMemberStatus.Accepted },
            new() { ProjectId = proj2.Id, UserId = devUser.Id, Role = ProjectMemberRole.Member, Status = ProjectMemberStatus.Accepted },
            new() { ProjectId = proj2.Id, UserId = qaUser.Id, Role = ProjectMemberRole.Member, Status = ProjectMemberStatus.Accepted },

            new() { ProjectId = proj3.Id, UserId = leadUser.Id, Role = ProjectMemberRole.Owner, Status = ProjectMemberStatus.Accepted },
            new() { ProjectId = proj3.Id, UserId = adminUser.Id, Role = ProjectMemberRole.Admin, Status = ProjectMemberStatus.Accepted },
            new() { ProjectId = proj3.Id, UserId = backendDev.Id, Role = ProjectMemberRole.Member, Status = ProjectMemberStatus.Accepted },

            new() { ProjectId = proj4.Id, UserId = pmUser.Id, Role = ProjectMemberRole.Owner, Status = ProjectMemberStatus.Accepted },
            new() { ProjectId = proj4.Id, UserId = adminUser.Id, Role = ProjectMemberRole.Admin, Status = ProjectMemberStatus.Accepted }
        };

        await context.ProjectMembers.AddRangeAsync(members);
        await context.SaveChangesAsync();

        // ──────────────────────────────────────────────
        // 4. BOARDS (Multiple Boards)
        // ──────────────────────────────────────────────
        var board1 = new Board
        {
            Id = Guid.Parse("b1111111-1111-1111-1111-111111111111"),
            ProjectId = proj1.Id,
            Name = "لوحة تطوير الكانبان (Sprint 1 - Web & API)",
            Description = "تطوير شاشات الكانبان التفاعلية، ومزامنة التواجد اللحظي، وضبط معمارية Clean Architecture.",
            BackgroundColor = "#1A2B4C",
            CreatedAt = DateTime.UtcNow.AddDays(-28)
        };

        var board2 = new Board
        {
            Id = Guid.Parse("b2222222-2222-2222-2222-222222222222"),
            ProjectId = proj1.Id,
            Name = "لوحة تحسينات التفاعل و UI/UX",
            Description = "تصميم الواجهات، الألوان، النوافذ المنبثقة ورسائل الخطأ باللغة العربية.",
            BackgroundColor = "#2D4A7A",
            CreatedAt = DateTime.UtcNow.AddDays(-20)
        };

        var board3 = new Board
        {
            Id = Guid.Parse("b3333333-3333-3333-3333-333333333333"),
            ProjectId = proj2.Id,
            Name = "لوحة واجهات تطبيق Flutter",
            Description = "شاشات تسجيل الدخول، عرض المشاريع، واللوحات بنظام المواد 3.",
            BackgroundColor = "#111E36",
            CreatedAt = DateTime.UtcNow.AddDays(-24)
        };

        var board4 = new Board
        {
            Id = Guid.Parse("b4444444-4444-4444-4444-444444444444"),
            ProjectId = proj4.Id,
            Name = "لوحة السيرفرات والأمن السيبراني",
            Description = "متابعة تكوين Docker، حماية API والتأكد من أمان JWT Tokens.",
            BackgroundColor = "#0F172A",
            CreatedAt = DateTime.UtcNow.AddDays(-9)
        };

        await context.Boards.AddRangeAsync(board1, board2, board3, board4);
        await context.SaveChangesAsync();

        // ──────────────────────────────────────────────
        // 5. COLUMNS
        // ──────────────────────────────────────────────
        var colTodo = new Column { Id = Guid.Parse("c1111111-1111-1111-1111-111111111111"), BoardId = board1.Id, Name = "قيد الانتظار (To Do)", Order = 1, WipLimit = 10 };
        var colInProgress = new Column { Id = Guid.Parse("c2222222-2222-2222-2222-222222222222"), BoardId = board1.Id, Name = "جاري العمل (In Progress)", Order = 2, WipLimit = 5 };
        var colReview = new Column { Id = Guid.Parse("c3333333-3333-3333-3333-333333333333"), BoardId = board1.Id, Name = "قيد المراجعة (Review)", Order = 3, WipLimit = 5 };
        var colDone = new Column { Id = Guid.Parse("c4444444-4444-4444-4444-444444444444"), BoardId = board1.Id, Name = "مكتمل (Done)", Order = 4 };

        var mcol1 = new Column { Id = Guid.Parse("c5555555-5555-5555-5555-555555555555"), BoardId = board3.Id, Name = "الواجهات المطلوبة", Order = 1 };
        var mcol2 = new Column { Id = Guid.Parse("c6666666-6666-6666-6666-666666666666"), BoardId = board3.Id, Name = "جاري التطوير في Flutter", Order = 2 };
        var mcol3 = new Column { Id = Guid.Parse("c7777777-7777-7777-7777-777777777777"), BoardId = board3.Id, Name = "مكتملة ومختبرة", Order = 3 };

        await context.Columns.AddRangeAsync(colTodo, colInProgress, colReview, colDone, mcol1, mcol2, mcol3);
        await context.SaveChangesAsync();

        // ──────────────────────────────────────────────
        // 6. CARDS (Rich Set of 15+ Cards)
        // ──────────────────────────────────────────────
        var cards = new List<Card>
        {
            // Completed Cards (Done)
            new()
            {
                Id = Guid.Parse("d1111111-1111-1111-1111-111111111111"),
                ColumnId = colDone.Id,
                Title = "بناء معمارية Clean Architecture وتكوين الـ Domain Entities",
                Description = "تقسيم الطبقات إلى Domain, Application, Infrastructure, API, Web وحفظ علاقات الكائنات.",
                Priority = Priority.Critical,
                Status = CardStatus.Done,
                AssigneeId = leadUser.Id,
                Order = 1,
                StoryPoints = 8,
                Labels = "Backend, Architecture, C#",
                CreatedAt = DateTime.UtcNow.AddDays(-28),
                DueDate = DateTime.UtcNow.AddDays(-20)
            },
            new()
            {
                Id = Guid.Parse("d2222222-2222-2222-2222-222222222222"),
                ColumnId = colDone.Id,
                Title = "تأمين النظام باستخدام JWT Bearer Tokens و Role-Based Authorization",
                Description = "إعداد حماية الـ Endpoints وتشفير كلمات المرور باستخدام BCrypt.",
                Priority = Priority.High,
                Status = CardStatus.Done,
                AssigneeId = backendDev.Id,
                Order = 2,
                StoryPoints = 5,
                Labels = "Security, JWT, API",
                CreatedAt = DateTime.UtcNow.AddDays(-25),
                DueDate = DateTime.UtcNow.AddDays(-18)
            },
            new()
            {
                Id = Guid.Parse("d3333333-3333-3333-3333-333333333333"),
                ColumnId = colDone.Id,
                Title = "إعداد قواعد البيانات PostgreSQL وتكوين EF Core Migrations",
                Description = "إنشاء جداول المستخدمين، المشاريع، اللوحات، الأعمدة، والبطاقات مع الفهارس المناسبة.",
                Priority = Priority.Critical,
                Status = CardStatus.Done,
                AssigneeId = leadUser.Id,
                Order = 3,
                StoryPoints = 5,
                Labels = "Database, PostgreSQL, EF Core",
                CreatedAt = DateTime.UtcNow.AddDays(-26),
                DueDate = DateTime.UtcNow.AddDays(-22)
            },

            // In Progress Cards
            new()
            {
                Id = Guid.Parse("d4444444-4444-4444-4444-444444444444"),
                ColumnId = colInProgress.Id,
                Title = "مزامنة التواجد اللحظي في اللوحة عبر SignalR Presence Hub",
                Description = "تتبع المتواجدين حالياً في اللوحة وبث تحديثات السحب والإفلات لحظياً لجميع الأعضاء.",
                Priority = Priority.High,
                Status = CardStatus.InProgress,
                AssigneeId = leadUser.Id,
                Order = 1,
                StoryPoints = 5,
                Labels = "SignalR, Realtime, Web",
                CreatedAt = DateTime.UtcNow.AddDays(-12),
                DueDate = DateTime.UtcNow.AddDays(1)
            },
            new()
            {
                Id = Guid.Parse("d5555555-5555-5555-5555-555555555555"),
                ColumnId = colInProgress.Id,
                Title = "تصميم واجهة Kanban التفاعلية ودعم السحب والإفلات (SortableJS)",
                Description = "تحسين التصميم باستخدام Vanilla CSS والبطاقات الديناميكية مع شريط البحث والفلترة.",
                Priority = Priority.High,
                Status = CardStatus.InProgress,
                AssigneeId = devUser.Id,
                Order = 2,
                StoryPoints = 5,
                Labels = "UI/UX, CSS, JavaScript",
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                DueDate = DateTime.UtcNow.AddDays(2)
            },
            new()
            {
                Id = Guid.Parse("d6666666-6666-6666-6666-666666666666"),
                ColumnId = colInProgress.Id,
                Title = "إعداد لوحة القيادة الشاملة (Dashboard Analytics)",
                Description = "توفير الإحصائيات الرسمية للرسوم البيانية ومتابعة تقدم المشاريع حسب الأعضاء.",
                Priority = Priority.Medium,
                Status = CardStatus.InProgress,
                AssigneeId = pmUser.Id,
                Order = 3,
                StoryPoints = 3,
                Labels = "Dashboard, Charts, Metrics",
                CreatedAt = DateTime.UtcNow.AddDays(-8),
                DueDate = DateTime.UtcNow.AddDays(3)
            },

            // In Review Cards
            new()
            {
                Id = Guid.Parse("d7777777-7777-7777-7777-777777777777"),
                ColumnId = colReview.Id,
                Title = "تعريب كافة المصطلحات والواجهات باللغة العربية الكاملة",
                Description = "مراجعة الترجمة في القوائم، الأزرار، خيارات البطاقات، ورسائل الخطأ التوضيحية.",
                Priority = Priority.Medium,
                Status = CardStatus.Todo,
                AssigneeId = devUser.Id,
                Order = 1,
                StoryPoints = 3,
                Labels = "Localization, RTL, Arabic",
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                DueDate = DateTime.UtcNow.AddDays(1)
            },
            new()
            {
                Id = Guid.Parse("d8888888-8888-8888-8888-888888888888"),
                ColumnId = colReview.Id,
                Title = "اختبارات الأمان وحماية النوافظ من هجمات CSRF و XSS",
                Description = "مراجعة مدخلات المستخدم وتأمين الروابط الخارجية وتشفير الاتصالات عبر HTTPS.",
                Priority = Priority.High,
                Status = CardStatus.Todo,
                AssigneeId = qaUser.Id,
                Order = 2,
                StoryPoints = 5,
                Labels = "Security, QA, Auditing",
                CreatedAt = DateTime.UtcNow.AddDays(-4),
                DueDate = DateTime.UtcNow.AddDays(2)
            },

            // Todo Cards
            new()
            {
                Id = Guid.Parse("d9999999-9999-9999-9999-999999999999"),
                ColumnId = colTodo.Id,
                Title = "إضافة اختبارات الوحدة (Unit Tests) لخدمات الـ Application Layer",
                Description = "تغطية كافة الحالات الخاصة بالبطاقات واللوحات باختبارات وحدة موثوقة باستخدام xUnit.",
                Priority = Priority.Medium,
                Status = CardStatus.Todo,
                AssigneeId = qaUser.Id,
                Order = 1,
                StoryPoints = 3,
                Labels = "QA, xUnit, Testing",
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                DueDate = DateTime.UtcNow.AddDays(5)
            },
            new()
            {
                Id = Guid.Parse("da111111-1111-1111-1111-111111111111"),
                ColumnId = colTodo.Id,
                Title = "دعم تصدير تقارير المشاريع بتنسيق PDF و Excel",
                Description = "إمكانية تصدير ملخص المهام والنشاطات اليومية لمدير المشروع بنقرة زر.",
                Priority = Priority.Low,
                Status = CardStatus.Todo,
                AssigneeId = pmUser.Id,
                Order = 2,
                StoryPoints = 2,
                Labels = "Reports, PDF, Export",
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                DueDate = DateTime.UtcNow.AddDays(7)
            },

            // Flutter App Cards
            new()
            {
                Id = Guid.Parse("db222222-2222-2222-2222-222222222222"),
                ColumnId = mcol2.Id,
                Title = "ربط شاشات Flutter مع REST API ومزامنة التوكن محلياً",
                Description = "تخزين الـ JWT بأمان عبر Flutter Secure Storage والتعامل مع حالة انقطاع الاتصال.",
                Priority = Priority.High,
                Status = CardStatus.InProgress,
                AssigneeId = devUser.Id,
                Order = 1,
                StoryPoints = 5,
                Labels = "Flutter, API, Storage",
                CreatedAt = DateTime.UtcNow.AddDays(-6),
                DueDate = DateTime.UtcNow.AddDays(1)
            },
            new()
            {
                Id = Guid.Parse("dc333333-3333-3333-3333-333333333333"),
                ColumnId = mcol3.Id,
                Title = "تصميم الشاشة الرئيسية القابلة للتمرير وقائمة المشاريع واللوحات",
                Description = "بناء واجهات متجاوبة لدعم كافة أحجام الشاشات باستخدام Material 3 Theme.",
                Priority = Priority.Medium,
                Status = CardStatus.Done,
                AssigneeId = devUser.Id,
                Order = 1,
                StoryPoints = 3,
                Labels = "Flutter, UI, Material3",
                CreatedAt = DateTime.UtcNow.AddDays(-15),
                DueDate = DateTime.UtcNow.AddDays(-5)
            }
        };

        await context.Cards.AddRangeAsync(cards);
        await context.SaveChangesAsync();

        // ──────────────────────────────────────────────
        // 7. COMMENTS
        // ──────────────────────────────────────────────
        var comments = new List<Comment>
        {
            new()
            {
                Id = Guid.NewGuid(),
                CardId = cards[3].Id, // SignalR card
                AuthorId = backendDev.Id,
                Content = "تم اختبار اتصال الـ Presence Bar مع الـ Hub ويعمل بكفاءة عالية جداً على البورت 5233!",
                CreatedAt = DateTime.UtcNow.AddHours(-10)
            },
            new()
            {
                Id = Guid.NewGuid(),
                CardId = cards[3].Id,
                AuthorId = leadUser.Id,
                Content = "ممتاز جداً يا خالد! تأكد من معالجة حالة قطع الاتصال وإعادة التوصيل التلقائي.",
                CreatedAt = DateTime.UtcNow.AddHours(-4)
            },
            new()
            {
                Id = Guid.NewGuid(),
                CardId = cards[4].Id, // SortableJS card
                AuthorId = devUser.Id,
                Content = "تمت إضافة التوافق التام مع الشاشات الصغيرة وأيقونة الخيارات السريعة للنقل والتعديل.",
                CreatedAt = DateTime.UtcNow.AddHours(-2)
            }
        };

        await context.Comments.AddRangeAsync(comments);
        await context.SaveChangesAsync();

        // ──────────────────────────────────────────────
        // 8. ACTIVITY LOGS
        // ──────────────────────────────────────────────
        var logs = new List<ActivityLog>
        {
            new()
            {
                Id = Guid.NewGuid(),
                ProjectId = proj1.Id,
                UserId = leadUser.Id,
                EntityType = "Project",
                Action = "Created",
                Description = "قام محمد قيس القباطي بإنشاء مشروع منصة مهام لإدارة المشاريع (Maham Web)",
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new()
            {
                Id = Guid.NewGuid(),
                ProjectId = proj1.Id,
                UserId = pmUser.Id,
                EntityType = "Board",
                Action = "Created",
                Description = "قامت ريم السقاف بإنشاء لوحة تطوير الكانبان (Sprint 1 - Web & API)",
                CreatedAt = DateTime.UtcNow.AddDays(-28)
            },
            new()
            {
                Id = Guid.NewGuid(),
                ProjectId = proj1.Id,
                UserId = devUser.Id,
                EntityType = "Card",
                Action = "UpdatedStatus",
                Description = "قامت سارة خالد بنقل البطاقة 'تصميم واجهة Kanban التفاعلية' إلى 'جاري العمل'",
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            },
            new()
            {
                Id = Guid.NewGuid(),
                ProjectId = proj1.Id,
                UserId = backendDev.Id,
                EntityType = "Card",
                Action = "CardCreated",
                Description = "قام خالد البعداني بإضافة تعليق جديد على بطاقة مزامنة التواجد اللحظي",
                CreatedAt = DateTime.UtcNow.AddHours(-10)
            }
        };

        await context.ActivityLogs.AddRangeAsync(logs);
        await context.SaveChangesAsync();
    }
}
