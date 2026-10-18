using ChurchYouthAttendance.Domain.Enum;
using System;

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
        public bool IsGeneralLeader => Role == UserRole.Admin;
        public DateTime CreatedAt { get; set; }
    }
}
