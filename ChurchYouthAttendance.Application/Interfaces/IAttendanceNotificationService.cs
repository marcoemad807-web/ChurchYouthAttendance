using ChurchYouthAttendance.Application.DTOs.Attendance;
using System.Threading;
using System.Threading.Tasks;

namespace ChurchYouthAttendance.Application.Interfaces
{
    public interface IAttendanceNotificationService
    {
        /// <summary>
        /// إرسال إشعار لحظي عبر SignalR لأمين الخدمة العام وخدام نفس الدفعة عند تسجيل الحضور
        /// </summary>
        Task NotifyAttendanceRecordedAsync(AttendanceNotificationDto notification, CancellationToken cancellationToken = default);

        /// <summary>
        /// إرسال إشعار لحظي عبر SignalR لأمين الخدمة العام وخدام نفس الدفعة عند إلغاء/حذف الحضور
        /// </summary>
        Task NotifyAttendanceRemovedAsync(int studentId, string studentName, int academicYear, int meetingId, string removedByServantName, CancellationToken cancellationToken = default);

        /// <summary>
        /// إرسال إشعار لحظي عبر SignalR للشباب عند نشر إعلان جديد من أمين الخدمة
        /// </summary>
        Task NotifyNewAnnouncementAsync(ChurchYouthAttendance.Application.DTOs.Announcement.AnnouncementDto announcement, CancellationToken cancellationToken = default);
    }
}
