using ChurchYouthAttendance.Domain.Enum;
using System;

namespace ChurchYouthAttendance.Application.DTOs.Student
{
    /// <summary>
    /// DTO لتعديل بيانات شاب
    /// </summary>
    public class UpdateStudentRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string? Username { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public DateOnly? DateOfBirth { get; set; }
        public UserGender Gender { get; set; } = UserGender.Male;
        public string? FatherOfConfession { get; set; }
        public int AcademicYear { get; set; } = 1;
        public string? College { get; set; }
    }
}
