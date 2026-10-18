using System;

namespace ChurchYouthAttendance.Application.DTOs.Meeting
{
    public class MeetingDto
    {
        public int Id { get; set; }
        public DateOnly Date { get; set; }
        public bool IsOpen { get; set; }
        public int TotalAttended { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
