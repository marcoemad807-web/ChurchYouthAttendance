using ChurchYouthAttendance.Application.DTOs.Attendance;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ChurchYouthAttendance.Application.Interfaces
{
    public interface IAttendanceService
    {
        Task<AttendanceResult> RecordAttendanceAsync(RecordAttendanceRequest request, CancellationToken cancellationToken = default);
        Task<AttendanceResult> RecordAttendanceByQrAsync(ScanQrAttendanceRequest request, CancellationToken cancellationToken = default);
        Task<AttendanceResult> RemoveAttendanceAsync(int studentId, int meetingId, int servantId, CancellationToken cancellationToken = default);
        Task<List<StudentAttendanceDto>> GetMeetingAttendanceByYearAsync(int meetingId, int academicYear, CancellationToken cancellationToken = default);

        /// <summary>
        /// كشف الغياب المانيول: يرجع كل الشباب مقسمين على دفعات مع حالة حضورهم في الاجتماع
        /// </summary>
        Task<ManualAttendanceSheet> GetManualAttendanceSheetAsync(int meetingId, CancellationToken cancellationToken = default);

        /// <summary>
        /// بحث في الشباب بالاسم مع حالة حضورهم في اجتماع محدد (للغياب المانيول)
        /// </summary>
        Task<List<StudentAttendanceDto>> SearchStudentsAsync(int meetingId, string searchTerm, CancellationToken cancellationToken = default);

        /// <summary>
        /// استخراج تقرير حضور وغياب شامل لفترة زمنية محددة (مع مراعاة صلاحيات الخادم والأمين)
        /// </summary>
        Task<AttendanceReportDto> GenerateAttendanceReportAsync(AttendanceReportRequest request, CancellationToken cancellationToken = default);
    }
}
