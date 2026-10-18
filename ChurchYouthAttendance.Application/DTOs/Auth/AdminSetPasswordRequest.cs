namespace ChurchYouthAttendance.Application.DTOs.Auth
{
    /// <summary>
    /// طلب تعيين كلمة مرور جديدة للمستخدم بواسطة أمين الخدمة (بدون الحاجة لكلمة المرور الحالية)
    /// </summary>
    public class AdminSetPasswordRequest
    {
        public int UserId { get; set; }
        public string UserType { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
