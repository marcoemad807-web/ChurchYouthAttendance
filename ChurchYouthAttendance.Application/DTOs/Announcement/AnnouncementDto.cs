using System;

namespace ChurchYouthAttendance.Application.DTOs.Announcement
{
    public class AnnouncementDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public int? TargetAcademicYear { get; set; }
        public string SenderServantName { get; set; } = string.Empty;
    }
}
