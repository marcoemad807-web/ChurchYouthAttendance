using ChurchYouthAttendance.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChurchYouthAttendance.Application.DTOs.Student
{
    /// <summary>
    /// DTO لتعديل بيانات شاب
    /// </summary>
    public class UpdateStudentRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty; // اسم المستخدم بالإنجليزية (مثل: mina.adel) - مطلوب
        public string PhoneNumber { get; set; } = string.Empty;
        public DateOnly DateOfBirth { get; set; }
        public UserGender Gender { get; set; }
        public string FatherOfConfession { get; set; } = string.Empty;
        public int AcademicYear { get; set; }
        public string College { get; set; } = string.Empty; // الكلية - مطلوبة
    }
}
