using ChurchYouthAttendance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChurchYouthAttendance.Infrastructure.Data.Configurations
{
    public class ServantConfiguration : IEntityTypeConfiguration<Servant>
    {
        public void Configure(EntityTypeBuilder<Servant> builder)
        {
            builder.ToTable("Servants");

            builder.Property(s => s.AssignedAcademicYear)
                .IsRequired(false);

            builder.Property(s => s.AssignedAcademicYears)
                .HasMaxLength(100)
                .IsRequired(false);

            builder.Property(s => s.ClassGender)
                .HasDefaultValue(0);

            builder.Ignore(s => s.IsGeneralLeader);

            builder.HasIndex(s => s.FullName);
        }
    }
}
