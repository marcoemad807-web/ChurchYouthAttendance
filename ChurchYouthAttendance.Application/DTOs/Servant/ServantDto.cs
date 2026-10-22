using ChurchYouthAttendance.Domain.Enum;
using System;
using System.Collections.Generic;

namespace ChurchYouthAttendance.Application.DTOs.Servant
{
    public class ServantDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Username { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string? PlainPassword { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public UserGender Gender { get; set; }
        public string FatherOfConfession { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public int? AssignedAcademicYear { get; set; }
        public List<int> AssignedAcademicYears { get; set; } = new();
        public int ClassGender { get; set; } = 0; // 0 = كلاهما, 1 = بنين, 2 = بنات
        public bool IsGeneralLeader => Role == UserRole.Admin;
        public DateTime CreatedAt { get; set; }
    }
}
