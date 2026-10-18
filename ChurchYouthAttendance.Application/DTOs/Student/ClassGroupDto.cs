using System.Collections.Generic;

namespace ChurchYouthAttendance.Application.DTOs.Student
{
    /// <summary>
    /// كشف فصل/دفعة للخدام وأمين الخدمة
    /// </summary>
    public class ClassGroupDto
    {
        public int AcademicYear { get; set; }
        public string YearLabel { get; set; } = string.Empty;
        public int TotalStudents { get; set; }
        public int RegisteredInAppCount { get; set; }
        public List<StudentDto> Students { get; set; } = new();
    }
}
