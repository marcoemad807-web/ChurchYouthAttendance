using ChurchYouthAttendance.Application.DTOs.Announcement;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ChurchYouthAttendance.Application.Interfaces
{
    public interface IAnnouncementService
    {
        Task<AnnouncementDto> CreateAnnouncementAsync(CreateAnnouncementRequest request, CancellationToken cancellationToken = default);
        Task<List<AnnouncementDto>> GetAnnouncementsForStudentAsync(int academicYear, int limit = 20, CancellationToken cancellationToken = default);
        Task<List<AnnouncementDto>> GetAllAnnouncementsAsync(int limit = 50, CancellationToken cancellationToken = default);
        Task<bool> UpdateStudentDeviceTokenAsync(UpdateDeviceTokenRequest request, CancellationToken cancellationToken = default);
    }
}
