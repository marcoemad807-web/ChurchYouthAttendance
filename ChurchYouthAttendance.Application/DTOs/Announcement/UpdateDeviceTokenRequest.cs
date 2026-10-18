namespace ChurchYouthAttendance.Application.DTOs.Announcement
{
    public class UpdateDeviceTokenRequest
    {
        public int StudentId { get; set; }
        public string DeviceToken { get; set; } = string.Empty;
    }
}
