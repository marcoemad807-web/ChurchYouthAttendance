using ChurchYouthAttendance.Domain.Entities;
using ChurchYouthAttendance.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChurchYouthAttendance.Infrastructure.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            // استراتيجية Table-Per-Concrete-Type (TPC) - لا يتم إنشاء جدول لـ User
            builder.UseTpcMappingStrategy();

            builder.HasKey(u => u.Id);

            builder.Property(u => u.FullName)
                .IsRequired()
                .HasMaxLength(150);

            // اسم المستخدم بالإنجليزية (مثل: mina.adel) - مطلوب وفريد
            builder.Property(u => u.Username)
                .HasMaxLength(100)
                .IsRequired();

            builder.HasIndex(u => u.Username)
                .IsUnique();

            // إعداد كائن القيمة PhoneNumber باستخدام Value Converter
            builder.Property(u => u.PhoneNumber)
                .HasConversion(
                    p => p.Value,
                    v => new PhoneNumber(v))
                .IsRequired()
                .HasMaxLength(20);

            builder.HasIndex(u => u.PhoneNumber)
                .IsUnique();

            builder.Property(u => u.PasswordHash)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(u => u.PlainPassword)
                .HasMaxLength(200)
                .IsRequired(false);

            builder.Property(u => u.DateOfBirth)
                .HasColumnType("date")
                .IsRequired();

            builder.Property(u => u.Gender)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(u => u.FatherOfConfession)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(u => u.Role)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(u => u.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");
        }
    }
}
