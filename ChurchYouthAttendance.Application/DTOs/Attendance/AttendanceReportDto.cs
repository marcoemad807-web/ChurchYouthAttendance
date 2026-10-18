using ChurchYouthAttendance.Domain.Enum;
using System;
using System.Collections.Generic;

namespace ChurchYouthAttendance.Application.DTOs.Attendance
{
    /// <summary>
    /// تقرير الحضور والغياب الشامل للفترة المحددة
    /// </summary>
    public class AttendanceReportDto
    {
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }
        public string RequestedByServantName { get; set; } = string.Empty;
        public UserRole ServantRole { get; set; }
        public string ScopeDescription { get; set; } = string.Empty; // مثل: "دفعة أولى - بنين وبنات" أو "جميع الفصول - بنين فقط"

        // ملخص الاجتماعات في الفترة
        public int TotalMeetingsCount { get; set; }
        public List<ReportMeetingSummaryDto> Meetings { get; set; } = new();

        // إحصائيات عامة
        public int TotalStudentsCount { get; set; }
        public int TotalAttendanceRecords { get; set; }
        public double AverageAttendancePerMeeting { get; set; }
        public double OverallAttendanceRate { get; set; } // نسبة مئوية إجمالية e.g. 78.5%

        // تفصيل حسب النوع (بنين / بنات)
        public GenderAttendanceSummary MaleSummary { get; set; } = new();
        public GenderAttendanceSummary FemaleSummary { get; set; } = new();

        // تفصيل حسب الفصول / الدفعات
        public List<ClassAttendanceSummary> ClassSummaries { get; set; } = new();

        // كشف تفصيلي بالشباب ونسب حضور كل شاب
        public List<StudentReportItemDto> Students { get; set; } = new();

        // قائمة الشباب المحتاجين افتقاد ومتابعة (نسبة حضورهم أقل من 50% أو غابوا الاجتماعات الأخيرة)
        public List<StudentReportItemDto> NeedsFollowUpStudents { get; set; } = new();
    }

    public class ReportMeetingSummaryDto
    {
        public int MeetingId { get; set; }
        public DateOnly Date { get; set; }
        public int AttendedCount { get; set; }
        public bool IsOpen { get; set; }
    }

    public class GenderAttendanceSummary
    {
        public int TotalStudents { get; set; }
        public int TotalAttendedRecords { get; set; }
        public double AttendanceRate { get; set; }
    }

    public class ClassAttendanceSummary
    {
        public int AcademicYear { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public int TotalStudents { get; set; }
        public int TotalAttendedRecords { get; set; }
        public double AttendanceRate { get; set; }
    }

    public class StudentReportItemDto
    {
        public int StudentId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public int AcademicYear { get; set; }
        public UserGender Gender { get; set; }
        public string College { get; set; } = string.Empty;
        public string FatherOfConfession { get; set; } = string.Empty;
        public bool IsAppRegistered { get; set; }

        public int AttendedMeetingsCount { get; set; }
        public int AbsentMeetingsCount { get; set; }
        public double AttendancePercentage { get; set; }

        // تفاصيل حالة الحضور في كل اجتماع من اجتماعات الفترة
        public List<StudentMeetingAttendanceDto> MeetingDetails { get; set; } = new();
    }

    public class StudentMeetingAttendanceDto
    {
        public int MeetingId { get; set; }
        public DateOnly MeetingDate { get; set; }
        public bool Attended { get; set; }
        public AttendanceMethod? Method { get; set; }
        public string? Notes { get; set; }
    }
}
