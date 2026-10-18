using System;

namespace ChurchYouthAttendance.Application.DTOs.Meeting
{
    /// <summary>
    /// يمثل خيار تاريخ جمعة محدد في فلتر تواريخ الغياب
    /// </summary>
    public class FridayOptionDto
    {
        public DateOnly Date { get; set; }
        public string FormattedDate { get; set; } = string.Empty;
        public bool IsCurrentWeek { get; set; }
        public int? MeetingId { get; set; }
        public bool HasMeeting => MeetingId.HasValue;
        public bool IsOpen { get; set; }
        public int TotalAttended { get; set; }
    }
}
