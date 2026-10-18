using ChurchYouthAttendance.Domain.Enum;
using ChurchYouthAttendance.Domain.ValueObjects;
using System;
using System.Collections.Generic;

namespace ChurchYouthAttendance.Domain.Entities
{
    public class Student : User
    {
        // Parameterless constructor for EF Core
        protected Student() { }

        // Constructor 1: لما الخادم يضيف الشاب في كشوفات الكنيسة أولاً (لسه مسجلش في الأبلكيشن)
        public Student(
            string fullName,
            string username,
            PhoneNumber phoneNumber,
            DateOnly dateOfBirth,
            UserGender gender,
            string fatherOfConfession,
            int academicYear,
            string college)
            : base(fullName, username, phoneNumber, dateOfBirth, gender, fatherOfConfession, UserRole.Student)
        {
            Validate(academicYear, college);

            AcademicYear = academicYear;
            College = college.Trim();
            IsAppRegistered = false;
        }

        private static void Validate(int academicYear, string college)
        {
            if (academicYear <= 0)
                throw new ArgumentException("السنة الدراسية يجب أن تكون رقماً موجباً.", nameof(academicYear));

            if (string.IsNullOrWhiteSpace(college))
                throw new ArgumentException("اسم الكلية مطلوب ولا يمكن أن يكون فارغاً.", nameof(college));
        }

        // Constructor 2: لو الشاب سجل مباشرة في الأبلكيشن بكلمة مرور
        public Student(
            string fullName,
            string username,
            PhoneNumber phoneNumber,
            string passwordHash,
            DateOnly dateOfBirth,
            UserGender gender,
            string fatherOfConfession,
            int academicYear,
            string college)
            : this(fullName, username, phoneNumber, dateOfBirth, gender, fatherOfConfession, academicYear, college)
        {
            CompleteAppRegistration(passwordHash);
        }

        public string College { get; set; } = string.Empty;
        public int AcademicYear { get; set; } // السنة الدراسية للشاب (مثلاً 1 لأولى، 2 لتانية، ...)

        // حالة التسجيل في الأبلكيشن والـ QR
        public bool IsAppRegistered { get; set; } = false;
        public DateTime? AppRegisteredAt { get; set; }
        public string? QrToken { get; set; }

        // رمز جهاز الموبايل لاستقبال Push Notifications (Firebase FCM)
        public string? DeviceToken { get; set; }

        // Navigation property: سجل حضور الشاب في الاجتماعات
        public List<Attendance> Attendances { get; set; } = new();

        // Domain Methods
        public void CompleteAppRegistration(string passwordHash)
        {
            PasswordHash = passwordHash;
            QrToken = Guid.NewGuid().ToString("N");
            IsAppRegistered = true;
            AppRegisteredAt = DateTime.UtcNow;
        }

        public void RegenerateQrToken()
        {
            QrToken = Guid.NewGuid().ToString("N");
        }
    }
}
