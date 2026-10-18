using ChurchYouthAttendance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChurchYouthAttendance.Infrastructure.Data.Configurations
{
    public class AttendanceConfiguration : IEntityTypeConfiguration<Attendance>
    {
        public void Configure(EntityTypeBuilder<Attendance> builder)
        {
            builder.ToTable("Attendances");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.AttendedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(a => a.Method)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(a => a.Notes)
                .HasMaxLength(500)
                .IsRequired(false);

            // إعداد Concurrency Token للتحكم في التزامن المتفائل
            builder.Property(a => a.RowVersion)
                .IsRowVersion();

            // علاقة الحضور مع الشاب (Student)
            builder.HasOne(a => a.Student)
                .WithMany(s => s.Attendances)
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.Restrict); // منع الحذف المتتالي لتجنب Multiple Cascade Paths في SQL Server

            // علاقة الحضور مع الاجتماع (Meeting)
            builder.HasOne(a => a.Meeting)
                .WithMany(m => m.Attendances)
                .HasForeignKey(a => a.MeetingId)
                .OnDelete(DeleteBehavior.Cascade);

            // علاقة الحضور مع الخادم الراصد (RecordedBy / Servant)
            builder.HasOne(a => a.RecordedBy)
                .WithMany(s => s.RecordedAttendances)
                .HasForeignKey(a => a.ServantId)
                .OnDelete(DeleteBehavior.Restrict); // منع الحذف المتتالي لتجنب Multiple Cascade Paths في SQL Server

            // منع تسجيل حضور الشاب في نفس الاجتماع أكثر من مرة (Unique Constraint)
            builder.HasIndex(a => new { a.StudentId, a.MeetingId })
                .IsUnique();

            // فهرس للبحث السريع بحسب وقت/تاريخ الحضور
            builder.HasIndex(a => a.AttendedAt);
        }
    }
}
