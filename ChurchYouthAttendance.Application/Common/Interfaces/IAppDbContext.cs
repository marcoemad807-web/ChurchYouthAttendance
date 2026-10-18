using ChurchYouthAttendance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace ChurchYouthAttendance.Application.Common.Interfaces
{
    public interface IAppDbContext
    {
        DbSet<Student> Students { get; }
        DbSet<Servant> Servants { get; }
        DbSet<Meeting> Meetings { get; }
        DbSet<Attendance> Attendances { get; }
        DbSet<Announcement> Announcements { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
