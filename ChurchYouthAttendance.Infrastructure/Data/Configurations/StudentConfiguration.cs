using ChurchYouthAttendance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChurchYouthAttendance.Infrastructure.Data.Configurations
{
    public class StudentConfiguration : IEntityTypeConfiguration<Student>
    {
        public void Configure(EntityTypeBuilder<Student> builder)
        {
            // جدول فرعي خاص بالطلاب ضمن استراتيجية TPT
            builder.ToTable("Students");

            builder.Property(s => s.College)
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(s => s.AcademicYear)
                .IsRequired();

            builder.Property(s => s.IsAppRegistered)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(s => s.AppRegisteredAt)
                .IsRequired(false);

            builder.Property(s => s.QrToken)
                .HasMaxLength(64)
                .IsRequired(false);

            builder.Property(s => s.DeviceToken)
                .HasMaxLength(500)
                .IsRequired(false);

            // فهرس فريد لـ QrToken للقيم غير الفارغة فقط (SQL Server Filtered Index)
            builder.HasIndex(s => s.QrToken)
                .IsUnique()
                .HasFilter("[QrToken] IS NOT NULL");

            // فهرس للبحث السريع باسم الطالب
            builder.HasIndex(s => s.FullName);
        }
    }
}
