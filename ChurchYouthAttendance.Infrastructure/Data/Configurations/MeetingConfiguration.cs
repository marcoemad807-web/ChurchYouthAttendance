using ChurchYouthAttendance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChurchYouthAttendance.Infrastructure.Data.Configurations
{
    public class MeetingConfiguration : IEntityTypeConfiguration<Meeting>
    {
        public void Configure(EntityTypeBuilder<Meeting> builder)
        {
            builder.ToTable("Meetings");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.Date)
                .HasColumnType("date")
                .IsRequired();

            // فهرس للبحث السريع بحسب تاريخ الاجتماع
            builder.HasIndex(m => m.Date);

            builder.Property(m => m.IsOpen)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(m => m.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");
        }
    }
}
