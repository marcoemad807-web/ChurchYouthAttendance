using ChurchYouthAttendance.Domain.Enum;
using System;

namespace ChurchYouthAttendance.Domain.Entities
{
    public class Attendance
    {
        // Parameterless constructor for EF Core
        protected Attendance() { }

        // Domain constructor
        public Attendance(int studentId, int meetingId, int servantId, AttendanceMethod method, string? notes = null)
        {
            Validate(studentId, meetingId, servantId);

            StudentId = studentId;
            MeetingId = meetingId;
            ServantId = servantId;
            Method = method;
            Notes = notes;
            AttendedAt = DateTime.UtcNow;
        }

        private static void Validate(int studentId, int meetingId, int servantId)
        {
            if (studentId <= 0)
                throw new ArgumentException("معرف الشاب (StudentId) غير صالح.", nameof(studentId));

            if (meetingId <= 0)
                throw new ArgumentException("معرف الاجتماع (MeetingId) غير صالح.", nameof(meetingId));

            if (servantId <= 0)
                throw new ArgumentException("معرف الخادم (ServantId) غير صالح.", nameof(servantId));
        }

        public int Id { get; set; }
        public DateTime AttendedAt { get; set; } = DateTime.UtcNow;
        public AttendanceMethod Method { get; set; } = AttendanceMethod.QrCode;
        public string? Notes { get; set; }

        // Concurrency Token للتحكم في التزامن المتفائل (Optimistic Concurrency Control)
        public byte[] RowVersion { get; set; } = [];

        // Foreign keys
        public int StudentId { get; set; }
        public int MeetingId { get; set; }
        public int ServantId { get; set; }

        // Navigation properties
        public Student Student { get; set; } = null!;
        public Meeting Meeting { get; set; } = null!;
        public Servant RecordedBy { get; set; } = null!;
    }
}
