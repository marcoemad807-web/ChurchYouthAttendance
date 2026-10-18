using System.Collections.Generic;

namespace ChurchYouthAttendance.Application.DTOs.Attendance
{
    /// <summary>
    /// كشف حضور دفعة كاملة (Tab واحد في شاشة الغياب المانيول)
    /// </summary>
    public class YearAttendanceGroup
    {
        public int AcademicYear { get; set; }
        public string YearLabel { get; set; } = string.Empty;  // مثلاً: "سنة أولى" أو "سنة رابعة"
        public int TotalStudents { get; set; }
        public int AttendedCount { get; set; }
        public int AbsentCount => TotalStudents - AttendedCount;
        public List<StudentAttendanceDto> Students { get; set; } = new();
    }

    /// <summary>
    /// ملخص كل الدفعات مع أعدادها (للـ Tabs في شاشة الغياب المانيول)
    /// </summary>
    public class ManualAttendanceSheet
    {
        public int MeetingId { get; set; }
        public string MeetingDate { get; set; } = string.Empty;
        public bool IsMeetingOpen { get; set; }
        public int TotalStudents { get; set; }
        public int TotalAttended { get; set; }
        public int TotalAbsent => TotalStudents - TotalAttended;
        public List<YearAttendanceGroup> Years { get; set; } = new();
    }
}
