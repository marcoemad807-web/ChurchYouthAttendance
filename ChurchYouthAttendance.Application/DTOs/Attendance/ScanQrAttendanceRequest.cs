namespace ChurchYouthAttendance.Application.DTOs.Attendance
{
    public class ScanQrAttendanceRequest
    {
        public string QrToken { get; set; } = string.Empty;
        public string? QrCodeData { get; set; }
        public int MeetingId { get; set; }
        public int ServantId { get; set; }
        public string? Notes { get; set; }
    }
}
