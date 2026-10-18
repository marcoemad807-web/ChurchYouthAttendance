using System;

namespace ChurchYouthAttendance.Application.DTOs.Meeting
{
    public class CreateMeetingRequest
    {
        public DateOnly Date { get; set; }
        public bool IsOpen { get; set; } = true;
    }
}
