using System;

namespace ChurchYouthAttendance.Application.DTOs.Meeting
{
    public class UpdateMeetingRequest
    {
        public DateOnly Date { get; set; }
        public bool IsOpen { get; set; }
    }
}
