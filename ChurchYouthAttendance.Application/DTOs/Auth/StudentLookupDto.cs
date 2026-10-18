using ChurchYouthAttendance.Domain.Enum;
using System;

namespace ChurchYouthAttendance.Application.DTOs.Auth
{
    /// <summary>
    /// كائن اقتراح بيانات الشاب المتاحة بالكشوفات لملء شاشة التسجيل تلقائياً
    /// </summary>
    public class StudentLookupDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateOnly DateOfBirth { get; set; }
        public UserGender Gender { get; set; }
        public string FatherOfConfession { get; set; } = string.Empty;
        public int AcademicYear { get; set; }
        public string College { get; set; } = string.Empty;
    }
}
