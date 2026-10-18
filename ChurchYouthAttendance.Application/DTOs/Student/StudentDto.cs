using ChurchYouthAttendance.Domain.Enum;
using System;

namespace ChurchYouthAttendance.Application.DTOs.Student
{
    public class StudentDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? PlainPassword { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public UserGender Gender { get; set; }
        public string FatherOfConfession { get; set; } = string.Empty;
        public int AcademicYear { get; set; }
        public string College { get; set; } = string.Empty;
        public bool IsAppRegistered { get; set; }
        public DateTime? AppRegisteredAt { get; set; }
        public string? QrToken { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
