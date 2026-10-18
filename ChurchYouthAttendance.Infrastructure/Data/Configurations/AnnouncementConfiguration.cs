using ChurchYouthAttendance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChurchYouthAttendance.Infrastructure.Data.Configurations
{
    public class AnnouncementConfiguration : IEntityTypeConfiguration<Announcement>
    {
        public void Configure(EntityTypeBuilder<Announcement> builder)
        {
            builder.ToTable("Announcements");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.Title)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(a => a.Body)
                .HasMaxLength(2000)
                .IsRequired();

            builder.Property(a => a.SentAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.HasOne(a => a.Sender)
                .WithMany()
                .HasForeignKey(a => a.SenderServantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(a => a.SentAt);
            builder.HasIndex(a => a.TargetAcademicYear);
        }
    }
}
