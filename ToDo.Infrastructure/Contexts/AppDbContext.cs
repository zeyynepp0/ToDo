using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ToDo.Domain.Entities;

namespace ToDo.Infrastructure.Contexts;

public class AppDbContext :DbContext


{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }


    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<StatusDefinition> StatusDefinitions { get; set; }
    public DbSet<ProjectStatus> ProjectStatuses { get; set; }
    public DbSet<ProjectStatusHistory> ProjectStatusHistories { get; set; }
    public DbSet<ProjectTask> ProjectTasks { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // --- User Ayarları ---
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id); // Primary Key

            // FullName sadece bir C# get propertysidir, veritabanında kolon oluşturulmaz.
            entity.Ignore(e => e.FullName);

            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Email).IsUnique(); // Aynı mail ile tekrar kayıt olunamasın

            
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.GeneralNotificationActive).HasDefaultValue(false);
            entity.Property(e => e.NotificationCount).HasDefaultValue(0);
            entity.Property(e => e.RegisteredDate).HasDefaultValueSql("GETDATE()"); // SQL Server için

            
            entity.HasOne(d => d.Role)
                  .WithMany(p => p.Users)
                  .HasForeignKey(d => d.RoleId)
                  .OnDelete(DeleteBehavior.Restrict); // Rol silinirse kullanıcılar silinmesin (Güvenlik)
        });

        // --- Role Ayarları ---
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            
            // CreatedDate otomatik atansın isterseniz:
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETDATE()");
        });

        //  SEED DATA (Başlangıç Verileri)
        // Bu ID'leri sabit veriyoruz ki her migration'da değişmesinler.
        var adminRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var userRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        modelBuilder.Entity<Role>().HasData(
            new Role
            {
                Id = adminRoleId,
                Name = "Admin",
                CreatedBy = "System",
                CreatedDate = new DateTime(2025, 1, 1), // Sabit tarih verelim migration sürekli değişmesin
                IsDeleted = false
            },
            new Role
            {
                Id = userRoleId,
                Name = "User", // Register metodunda bu ismi arayacağız!
                CreatedBy = "System",
                CreatedDate = new DateTime(2025, 1, 1),
                IsDeleted = false
            }
        );


        //---- project ----
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            //entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETDATE()");// otomatik atansın
            entity.HasQueryFilter(x => !x.IsDeleted);// soft delete
            //entity.HasOne(d => d.User)
            //      .WithMany(p => p.Projects)
            //      .HasForeignKey(d => d.UserId)
            //      .HasQueryFilter(x => !x.IsDeleted);  // soft delete
        });


        //---- StatusDefinition ----
        modelBuilder.Entity<StatusDefinition>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(x => x.SystemCode).HasMaxLength(50);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETDATE()");
            entity.HasIndex(x => x.SystemCode).IsUnique(false);
            entity.HasQueryFilter(x => !x.IsDeleted);
        });

        //----Role seed
        var sNew = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1");
        var sAnalysis = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2");
        var sProgress = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3");
        var sTestDone = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4");
        var sDone = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5");


        modelBuilder.Entity<StatusDefinition>().HasData(
    new StatusDefinition
    {
        Id = sNew,
        Name = "Yeni",
        SystemCode = "NEW",
        IsSystem = true,
        IsActive = true,
        CreatedBy = "System",
        CreatedDate = new DateTime(2025, 1, 1),
        IsDeleted = false,
         CreatedByuserId = "System"
    },
    new StatusDefinition
    {
        Id = sAnalysis,
        Name = "Analiz",
        SystemCode = "ANALYSIS",
        IsSystem = true,
        IsActive = true,
        CreatedBy = "System",
        CreatedDate = new DateTime(2025, 1, 1),
        IsDeleted = false,
        CreatedByuserId = "System"
    },
    new StatusDefinition
    {
        Id = sProgress,
        Name = "Devam",
        SystemCode = "IN_PROGRESS",
        IsSystem = true,
        IsActive = true,
        CreatedBy = "System",
        CreatedDate = new DateTime(2025, 1, 1),
        IsDeleted = false,
        CreatedByuserId = "System"
    },
    new StatusDefinition
    {
        Id = sTestDone,
        Name = "Test Tamamlandı",
        SystemCode = "TEST_DONE",
        IsSystem = true,
        IsActive = true,
        CreatedBy = "System",
        CreatedDate = new DateTime(2025, 1, 1),
        IsDeleted = false,
        CreatedByuserId = "System"
    },
    new StatusDefinition
    {
        Id = sDone,
        Name = "Tamamlandı",
        SystemCode = "DONE",
        IsSystem = true,
        IsActive = true,
        CreatedBy = "System",
        CreatedDate = new DateTime(2025, 1, 1),
        IsDeleted = false,
        CreatedByuserId = "System"
    }
);

        //---- ProjectStatus ----
        modelBuilder.Entity<ProjectStatus>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETDATE()");
            entity.HasQueryFilter(x => !x.IsDeleted);

            entity.HasOne(d => d.Project)
                  .WithMany(p => p.ProjectStatuses)
                  .HasForeignKey(d => d.ProjectId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.StatusDefinition)
                  .WithMany(p => p.ProjectStatuses)
                  .HasForeignKey(d => d.StatusDefinitionId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.ProjectId, x.StatusDefinitionId }).IsUnique(); //aynı projede aynı statusDefiniton 1 kere tanımlı olsun
            entity.HasIndex(x => new { x.ProjectId, x.OrderNo }).IsUnique(); //sıralama için
        });


        //---- ProjectStatusHistory ----
        modelBuilder.Entity<ProjectStatusHistory>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasOne(x => x.Project)
             .WithMany(p => p.StatusHistory)
             .HasForeignKey(x => x.ProjectId)
             .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(h => !h.Project.IsDeleted);

            entity.HasOne(x => x.FromProjectStatus)
                .WithMany()
                .HasForeignKey(x => x.FromProjectStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ToProjectStatus)
                .WithMany()
                .HasForeignKey(x => x.ToProjectStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.ProjectId, x.ChangedAt });
        });


        //---- ProjectTask ----
        modelBuilder.Entity<ProjectTask>(entity =>
        {
            entity.Property(x => x.Title).IsRequired().HasMaxLength(250);

            entity.HasQueryFilter(x => !x.IsDeleted);

            entity.HasOne(x => x.Project)
                .WithMany(p => p.Tasks)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ProjectStatus)
                .WithMany(s => s.Tasks)
                .HasForeignKey(x => x.ProjectStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            // self FK (parent-child)
            entity.HasOne(x => x.ParentTask)
                .WithMany(x => x.Children)
                .HasForeignKey(x => x.ParentTaskId)
                .OnDelete(DeleteBehavior.Restrict);

          
            entity.HasIndex(x => new { x.ProjectId, x.ProjectStatusId, x.ParentTaskId });
            entity.HasIndex(x => new { x.ProjectStatusId, x.OrderNo });
            entity.HasIndex(x => new { x.ProjectStatusId, x.ParentTaskId, x.OrderNo }).IsUnique();

        });

        base.OnModelCreating(modelBuilder);
    }
}
