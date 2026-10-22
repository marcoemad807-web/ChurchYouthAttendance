using ChurchYouthAttendance.Domain.Enum;
using System.Collections.Generic;

namespace ChurchYouthAttendance.Application.DTOs.Auth
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Username { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public int? AssignedAcademicYear { get; set; }  // الدفعة الأولى المسؤول عنها الخادم
        public List<int> AssignedAcademicYears { get; set; } = new(); // جميع الدفعات المسؤول عنها الخادم
        public int ClassGender { get; set; } = 0;      // نوع الفصل: 0 = كلاهما, 1 = بنين, 2 = بنات
        public int? AcademicYear { get; set; }          // دفعة الشاب (لو مسجل كشاب)
        public string? QrToken { get; set; }           // كود الـ QR الخاص بالشاب لعرضه في التطبيق
        public bool IsGeneralLeader => Role == UserRole.Admin;
    }
}
