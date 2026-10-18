using System;
using System.Collections.Generic;

namespace ChurchYouthAttendance.Domain.Entities
{
    public class Meeting
    {
        // Parameterless constructor for EF Core
        protected Meeting() { }

        // Domain constructor
        public Meeting(DateOnly date)
        {
            Validate(date);

            Date = date;
            IsOpen = true;
            CreatedAt = DateTime.UtcNow;
        }

        private static void Validate(DateOnly date)
        {
            if (date == default)
                throw new ArgumentException("تاريخ الاجتماع غير صالح.", nameof(date));
        }

        public int Id { get; set; }
        public DateOnly Date { get; set; }
        public bool IsOpen { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public List<Attendance> Attendances { get; set; } = new();

        // Domain Methods
        public void Close()
        {
            IsOpen = false;
        }

        public void Open()
        {
            IsOpen = true;
        }
    }
}
