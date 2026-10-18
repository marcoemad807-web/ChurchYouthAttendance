using ChurchYouthAttendance.Domain.Enum;
using System;

namespace ChurchYouthAttendance.Application.DTOs.Attendance
{
    /// <summary>
    /// طلب إنشاء تقرير حضور وغياب لفترة زمنية محددة
    /// </summary>
    public class AttendanceReportRequest
    {
        /// <summary>
        /// معرّف الخادم طالب التقرير للتحقق من صلاحياته (خادم فصل أم أمين خدمة عام)
        /// </summary>
        public int ServantId { get; set; }

        /// <summary>
        /// بداية الفترة الزمنية للتقرير
        /// </summary>
        public DateOnly FromDate { get; set; }

        /// <summary>
        /// نهاية الفترة الزمنية للتقرير
        /// </summary>
        public DateOnly ToDate { get; set; }

        /// <summary>
        /// السنة الدراسية / الدفعة (متاح فقط لأمين الخدمة العام، لخادم الفصل يتم تقييده تلقائياً بدفعته فقط)
        /// </summary>
        public int? AcademicYear { get; set; }

        /// <summary>
        /// النوع (بنين / بنات / الكل): متاح لأمين الخدمة العام وللخادم لتحديد نطاق التقرير
        /// </summary>
        public UserGender? Gender { get; set; }
    }
}
