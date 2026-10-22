using System;

namespace ChurchYouthAttendance.Application.DTOs.FollowUp
{
    /// <summary>
    /// كشف الافتقاد: شاب متغيب عن الاجتماعات ويحتاج لمتابعة وافتقاد من الخادم
    /// </summary>
    public class MissingStudentDto
    {
        public int StudentId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public int AcademicYear { get; set; }
        public int Gender { get; set; }
        public string? College { get; set; }
        public string FatherOfConfession { get; set; } = string.Empty;
        public int ConsecutiveAbsenceWeeks { get; set; }
        public DateOnly? LastAttendedDate { get; set; }
    }
}
