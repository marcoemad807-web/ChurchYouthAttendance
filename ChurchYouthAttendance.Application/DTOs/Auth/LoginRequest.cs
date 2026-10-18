namespace ChurchYouthAttendance.Application.DTOs.Auth
{
    public class LoginRequest
    {
        /// <summary>
        /// اسم المستخدم بالإنجليزي (مثل: mina.adel) أو رقم الموبايل
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// خاصية مساعدة للتوافق في حال إرسال PhoneNumber من تطبيق قديم
        /// </summary>
        public string? PhoneNumber
        {
            get => Username;
            set { if (!string.IsNullOrWhiteSpace(value) && string.IsNullOrWhiteSpace(Username)) Username = value; }
        }

        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// رمز جهاز الموبايل (Firebase FCM Token) لتحديثه واستقبال الإشعارات تلقائياً
        /// </summary>
        public string? DeviceToken { get; set; }
    }
}
