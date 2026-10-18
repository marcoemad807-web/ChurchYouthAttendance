using ChurchYouthAttendance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChurchYouthAttendance.Infrastructure.Data.Configurations
{
    public class ServantConfiguration : IEntityTypeConfiguration<Servant>
    {
        public void Configure(EntityTypeBuilder<Servant> builder)
        {
            // جدول فرعي خاص بالخدام وأمناء الخدمة ضمن استراتيجية TPT
            builder.ToTable("Servants");

            builder.Property(s => s.AssignedAcademicYear)
                .IsRequired(false);

            // استبعاد الخاصية المحسوبة IsGeneralLeader حتى لا يحاول EF Core إنشاء عمود لها
            builder.Ignore(s => s.IsGeneralLeader);

            // فهرس للبحث السريع باسم الخادم
            builder.HasIndex(s => s.FullName);
        }
    }
}
