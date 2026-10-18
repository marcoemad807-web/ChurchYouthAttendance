using ChurchYouthAttendance.Application.Common.Interfaces;
using ChurchYouthAttendance.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChurchYouthAttendance.Infrastructure.Data
{
    public class AppDbContext : DbContext, IAppDbContext
    {
        // تعريف نص الاتصال كـ متغير (Variable / Constant)
        private const string ConnectionString = "Server=.;Database=ChurchYouthAttendanceDb;Trusted_Connection=True;TrustServerCertificate=True;";

        // Constructor افتراضي (مفيد للـ Migrations أو الـ Design-time)
        public AppDbContext()
        {
        }

        // Constructor للحقن عبر Dependency Injection
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Student> Students => Set<Student>();
        public DbSet<Servant> Servants => Set<Servant>();
        public DbSet<Meeting> Meetings => Set<Meeting>();
        public DbSet<Attendance> Attendances => Set<Attendance>();
        public DbSet<Announcement> Announcements => Set<Announcement>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));

            // في حال لم يتم تمرير الـ Options من الـ Dependency Injection في Program.cs
            if (!optionsBuilder.IsConfigured)
            {
                
                optionsBuilder.UseSqlServer(ConnectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // تطبيق كافة الـ Configurations المعرفة في الـ Assembly تلقائياً
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
