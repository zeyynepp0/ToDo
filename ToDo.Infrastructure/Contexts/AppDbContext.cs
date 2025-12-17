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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // --- User Ayarları ---
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id); // Primary Key

            // FullName sadece bir C# get property'sidir, veritabanında kolon oluşturulmaz.
            entity.Ignore(e => e.FullName);

            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Email).IsUnique(); // Aynı mail ile tekrar kayıt olunamasın

            // Default Değerler
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.GeneralNotificationActive).HasDefaultValue(false);
            entity.Property(e => e.NotificationCount).HasDefaultValue(0);
            entity.Property(e => e.RegisteredDate).HasDefaultValueSql("GETDATE()"); // SQL Server için

            // İlişki: Bir User'ın bir Role'ü vardır.
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

        // 3. SEED DATA (Başlangıç Verileri)
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

        base.OnModelCreating(modelBuilder);
    }
}
