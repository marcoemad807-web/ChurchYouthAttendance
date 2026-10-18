using ChurchYouthAttendance.API.Hubs;
using ChurchYouthAttendance.Application.DTOs.Attendance;
using ChurchYouthAttendance.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ChurchYouthAttendance.API.Services
{
    public class SignalRAttendanceNotificationService : IAttendanceNotificationService
    {
        private readonly IHubContext<AttendanceHub> _hubContext;

        public SignalRAttendanceNotificationService(IHubContext<AttendanceHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifyAttendanceRecordedAsync(AttendanceNotificationDto notification, CancellationToken cancellationToken = default)
        {
            // إرسال الإشعار اللحظي إلى:
            // 1. أمين الخدمة العام (Admins)
            // 2. خدام نفس الدفعة/الفصل (Year_{AcademicYear})
            var targetGroups = new List<string>
            {
                "Admins",
                AttendanceHub.GetYearGroupName(notification.AcademicYear)
            };

            await _hubContext.Clients
                .Groups(targetGroups)
                .SendAsync("AttendanceRecorded", notification, cancellationToken);
        }

        public async Task NotifyAttendanceRemovedAsync(int studentId, string studentName, int academicYear, int meetingId, string removedByServantName, CancellationToken cancellationToken = default)
        {
            var targetGroups = new List<string>
            {
                "Admins",
                AttendanceHub.GetYearGroupName(academicYear)
            };

            var payload = new
            {
                StudentId = studentId,
                StudentName = studentName,
                AcademicYear = academicYear,
                MeetingId = meetingId,
                RemovedByServantName = removedByServantName,
                IsAttended = false
            };

            await _hubContext.Clients
                .Groups(targetGroups)
                .SendAsync("AttendanceRemoved", payload, cancellationToken);
        }

        public async Task NotifyNewAnnouncementAsync(ChurchYouthAttendance.Application.DTOs.Announcement.AnnouncementDto announcement, CancellationToken cancellationToken = default)
        {
            var targetGroups = new List<string> { "Admins" };

            if (announcement.TargetAcademicYear.HasValue)
            {
                // إعلان مخصص لدفعة معينة: يصل لخدام وطلاب تلك الدفعة
                targetGroups.Add(AttendanceHub.GetYearGroupName(announcement.TargetAcademicYear.Value));
            }
            else
            {
                // إعلان عام موجه لكل الشباب
                targetGroups.Add("Students");
            }

            await _hubContext.Clients
                .Groups(targetGroups)
                .SendAsync("OnNewAnnouncement", announcement, cancellationToken);
        }
    }
}
