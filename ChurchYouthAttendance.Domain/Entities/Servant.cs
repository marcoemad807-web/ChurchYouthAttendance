using ChurchYouthAttendance.Domain.Enum;
using ChurchYouthAttendance.Domain.ValueObjects;
using System;
using System.Collections.Generic;

namespace ChurchYouthAttendance.Domain.Entities
{
    public class Servant : User
    {
        // Parameterless constructor for EF Core
        protected Servant() { }

        // Domain Constructor للخادم أو أمين الخدمة
        public Servant(
            string fullName,
            string username,
            PhoneNumber phoneNumber,
            string passwordHash,
            DateOnly dateOfBirth,
            UserGender gender,
            string fatherOfConfession,
            UserRole role,
            int? assignedAcademicYear = null)
            : base(fullName, username, phoneNumber, dateOfBirth, gender, fatherOfConfession, role, passwordHash)
        {
            Validate(role, assignedAcademicYear);

            // لو أمين خدمة (Admin) مش مقيد بدفعة، لو خادم عادي (Servant) بيتحدد له الدفعة
            AssignedAcademicYear = (role == UserRole.Admin) ? null : assignedAcademicYear;
        }

        private static void Validate(UserRole role, int? assignedAcademicYear)
        {
            if (role == UserRole.Student)
                throw new ArgumentException("لا يمكن للخادم أن يحمل رتبة مخدوم (Student).", nameof(role));

            if (role == UserRole.Servant && assignedAcademicYear == null)
            {
                throw new ArgumentException("يجب تحديد الدفعة / السنة الدراسية المسندة للخادم.", nameof(assignedAcademicYear));
            }
        }

        // الدفعة/السنة الدراسية المسندة للخادم (null لأمين الخدمة)
        public int? AssignedAcademicYear { get; set; }

        // خاصية لمعرفة هل هو أمين الخدمة العام
        public bool IsGeneralLeader => Role == UserRole.Admin;

        // Navigation property: سجل كل الحضور الذي رصده هذا الخادم للشباب
        public List<Attendance> RecordedAttendances { get; set; } = new();
    }
}
