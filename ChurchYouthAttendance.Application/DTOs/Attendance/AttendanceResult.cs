using ChurchYouthAttendance.Domain.Enum;
using System;

namespace ChurchYouthAttendance.Application.DTOs.Attendance
{
    public class StudentAttendanceDto
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public int AcademicYear { get; set; }
        public int Gender { get; set; }
        public string? College { get; set; }
        public bool IsAttended { get; set; }
        public DateTime? AttendedAt { get; set; }
        public AttendanceMethod? Method { get; set; }
        public string? RecordedByServantName { get; set; }
        public string? Notes { get; set; }
    }

    public enum AttendanceResultStatus
    {
        Success,
        AlreadyRecorded,
        ConcurrencyConflict,
        NotFound,
        MeetingClosed,
        ValidationError
    }

    public class AttendanceResult
    {
        public bool Succeeded => Status == AttendanceResultStatus.Success;
        public AttendanceResultStatus Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public AttendanceNotificationDto? Data { get; set; }

        public static AttendanceResult Success(AttendanceNotificationDto data, string message = "تم تسجيل الحضور بنجاح.") =>
            new() { Status = AttendanceResultStatus.Success, Message = message, Data = data };

        public static AttendanceResult AlreadyRecorded(string message = "تم تسجيل حضور هذا الشاب بالفعل في هذا الاجتماع بواسطة خادم آخر.") =>
            new() { Status = AttendanceResultStatus.AlreadyRecorded, Message = message };

        public static AttendanceResult ConcurrencyConflict(string message = "حدث تعارض أثناء محاولة التعديل، تم تحديث السجل من مستخدم آخر.") =>
            new() { Status = AttendanceResultStatus.ConcurrencyConflict, Message = message };

        public static AttendanceResult NotFound(string message) =>
            new() { Status = AttendanceResultStatus.NotFound, Message = message };

        public static AttendanceResult MeetingClosed(string message = "الاجتماع مغلق ولا يمكن تسجيل الحضور فيه.") =>
            new() { Status = AttendanceResultStatus.MeetingClosed, Message = message };

        public static AttendanceResult Failure(string message) =>
            new() { Status = AttendanceResultStatus.ValidationError, Message = message };
    }
}
