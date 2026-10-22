using ChurchYouthAttendance.Domain.Enum;
using ChurchYouthAttendance.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

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
            int? assignedAcademicYear = null,
            int classGender = 0,
            string? assignedAcademicYears = null)
            : base(fullName, username, phoneNumber, dateOfBirth, gender, fatherOfConfession, role, passwordHash)
        {
            if (role == UserRole.Student)
                throw new ArgumentException("لا يمكن للخادم أن يحمل رتبة مخدوم (Student).", nameof(role));

            if (role == UserRole.Admin)
            {
                AssignedAcademicYear = null;
                AssignedAcademicYears = null;
                ClassGender = 0;
            }
            else
            {
                var yearsList = ParseAcademicYears(assignedAcademicYears, assignedAcademicYear);
                if (role == UserRole.Servant && yearsList.Count == 0)
                {
                    throw new ArgumentException("يجب تحديد الدفعة / السنة الدراسية المسندة للخادم.", nameof(assignedAcademicYear));
                }

                AssignedAcademicYear = yearsList.Count > 0 ? yearsList[0] : null;
                AssignedAcademicYears = yearsList.Count > 0 ? string.Join(",", yearsList) : null;
                ClassGender = classGender;
            }
        }

        // الدفعة/السنة الدراسية الأساسية المسندة للخادم (null لأمين الخدمة)
        public int? AssignedAcademicYear { get; set; }

        // الدفعات/السنوات الدراسية المتعددة المسندة للخادم مفصولة بفواصل مثل "1,2" (null لأمين الخدمة)
        public string? AssignedAcademicYears { get; set; }

        // نوع الفصل المسند للخادم: 0 = كلاهما (بنين وبنات), 1 = بنين, 2 = بنات
        public int ClassGender { get; set; } = 0;

        // خاصية لمعرفة هل هو أمين الخدمة العام
        public bool IsGeneralLeader => Role == UserRole.Admin;

        // دالة مساعدة لتحليل وتحويل السلسلة إلى قائمة أرقام الدفعات
        public static List<int> ParseAcademicYears(string? assignedAcademicYears, int? singleYear = null)
        {
            var list = new List<int>();
            if (!string.IsNullOrWhiteSpace(assignedAcademicYears))
            {
                var parts = assignedAcademicYears.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var p in parts)
                {
                    if (int.TryParse(p.Trim(), out var val) && val > 0 && !list.Contains(val))
                    {
                        list.Add(val);
                    }
                }
            }

            if (singleYear.HasValue && singleYear.Value > 0 && !list.Contains(singleYear.Value))
            {
                list.Add(singleYear.Value);
            }

            list.Sort();
            return list;
        }

        // دالة مساعدة لتحويل السلسلة إلى قائمة أرقام الدفعات
        public List<int> GetAssignedAcademicYearsList()
        {
            if (Role == UserRole.Admin) return new List<int>();
            return ParseAcademicYears(AssignedAcademicYears, AssignedAcademicYear);
        }

        // فحص هل الخادم مسؤول عن دفعة معينة
        public bool IsAssignedToYear(int academicYear)
        {
            if (Role == UserRole.Admin) return true;
            return GetAssignedAcademicYearsList().Contains(academicYear);
        }

        // تعيين الدفعات ونوع الفصل المسند للخادم
        public void SetAssignedClasses(IEnumerable<int>? academicYears, int classGender)
        {
            if (Role == UserRole.Admin)
            {
                AssignedAcademicYear = null;
                AssignedAcademicYears = null;
                ClassGender = 0;
                return;
            }

            var list = academicYears?.Where(y => y > 0).Distinct().OrderBy(y => y).ToList() ?? new List<int>();
            if (list.Count == 0)
            {
                throw new ArgumentException("يجب تحديد دفعة واحدة على الأقل مسندة للخادم.");
            }

            AssignedAcademicYear = list[0];
            AssignedAcademicYears = string.Join(",", list);
            ClassGender = classGender;
        }

        // Navigation property: سجل كل الحضور الذي سجله هذا الخادم للشباب
        public List<Attendance> RecordedAttendances { get; set; } = new();
    }
}
