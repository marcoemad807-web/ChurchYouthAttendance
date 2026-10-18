using ChurchYouthAttendance.Domain.Enum;
using System;

namespace ChurchYouthAttendance.Application.DTOs.Servant
{
    public class CreateServantRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty; // اسم المستخدم بالإنجليزية (مثل: peter.magdy) - مطلوب
        public string PhoneNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public DateOnly DateOfBirth { get; set; }
        public UserGender Gender { get; set; }
        public string FatherOfConfession { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.Servant;
        public int? AssignedAcademicYear { get; set; }
    }
}
