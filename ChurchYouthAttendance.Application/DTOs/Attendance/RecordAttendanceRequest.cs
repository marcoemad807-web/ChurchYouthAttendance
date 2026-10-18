using ChurchYouthAttendance.Domain.Enum;

namespace ChurchYouthAttendance.Application.DTOs.Attendance
{
    public class RecordAttendanceRequest
    {
        public int StudentId { get; set; }
        public int MeetingId { get; set; }
        public int ServantId { get; set; }
        public AttendanceMethod Method { get; set; } = AttendanceMethod.QrCode;
        public string? Notes { get; set; }
    }
}
