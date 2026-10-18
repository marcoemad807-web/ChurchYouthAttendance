using ChurchYouthAttendance.Domain.Enum;

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
        public int? AssignedAcademicYear { get; set; }  // الدفعة المسئول عنها الخادم
        public int? AcademicYear { get; set; }          // دفعة الشاب (لو مسجل كشاب)
        public string? QrToken { get; set; }           // كود الـ QR الخاص بالشاب لعرضه في التطبيق
        public bool IsGeneralLeader => Role == UserRole.Admin;
    }
}
