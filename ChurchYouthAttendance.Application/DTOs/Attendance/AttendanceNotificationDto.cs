using ChurchYouthAttendance.Domain.Enum;
using System;

namespace ChurchYouthAttendance.Application.DTOs.Attendance
{
    public class AttendanceNotificationDto
    {
        public int AttendanceId { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int AcademicYear { get; set; }
        public int MeetingId { get; set; }
        public string RecordedByServantName { get; set; } = string.Empty;
        public DateTime AttendedAt { get; set; }
        public AttendanceMethod Method { get; set; }
        public string? Notes { get; set; }
        public bool IsAttended { get; set; } = true;
    }
}
