using ChurchYouthAttendance.Domain.Enum;
using System;
using System.Collections.Generic;

namespace ChurchYouthAttendance.Application.DTOs.Servant
{
    public class UpdateServantRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateOnly DateOfBirth { get; set; }
        public UserGender Gender { get; set; }
        public string FatherOfConfession { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public int? AssignedAcademicYear { get; set; }
        public List<int>? AssignedAcademicYears { get; set; }
        public string? AssignedYears { get; set; }
        public int ClassGender { get; set; } = 0; // 0 = كلاهما, 1 = بنين, 2 = بنات
    }
}
