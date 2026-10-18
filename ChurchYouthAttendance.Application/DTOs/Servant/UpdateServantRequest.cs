using ChurchYouthAttendance.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChurchYouthAttendance.Application.DTOs.Servant
{
    public class UpdateServantRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty; // اسم المستخدم بالإنجليزية (مثل: peter.magdy) - مطلوب
        public string PhoneNumber { get; set; } = string.Empty;
        public DateOnly DateOfBirth { get; set; }
        public UserGender Gender { get; set; }
        public string FatherOfConfession { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public int? AssignedAcademicYear { get; set; }
    }
}
