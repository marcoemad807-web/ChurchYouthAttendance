using ChurchYouthAttendance.Domain.Enum;
using System;

namespace ChurchYouthAttendance.Application.DTOs.Auth
{
    /// <summary>
    /// طلب تسجيل الشاب لنفسه في التطبيق لأول مرة
    /// </summary>
    public class StudentSelfRegisterRequest
    {
        /// <summary>
        /// معرّف الشاب في حال اختار اسمه من قائمة الاقتراحات المفلترة مسبقاً (اختياري)
        /// </summary>
        public int? ExistingStudentId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty; // اسم المستخدم بالإنجليزية (مثل: mina.adel)
        public string PhoneNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public DateOnly DateOfBirth { get; set; }
        public UserGender Gender { get; set; }
        public string FatherOfConfession { get; set; } = string.Empty;
        public int AcademicYear { get; set; }
        public string College { get; set; } = string.Empty; // الكلية - مطلوبة
        public string? DeviceToken { get; set; }
    }
}
