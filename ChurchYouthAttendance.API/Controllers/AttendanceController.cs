using ChurchYouthAttendance.Application.DTOs.Attendance;
using ChurchYouthAttendance.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace ChurchYouthAttendance.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;

        public AttendanceController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        /// <summary>
        /// تسجيل حضور مخدوم (شاب) في اجتماع
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> RecordAttendance([FromBody] RecordAttendanceRequest request, CancellationToken cancellationToken)
        {
            var result = await _attendanceService.RecordAttendanceAsync(request, cancellationToken);

            return result.Status switch
            {
                AttendanceResultStatus.Success => Ok(result),
                AttendanceResultStatus.AlreadyRecorded => Conflict(new { message = result.Message, status = result.Status.ToString() }),
                AttendanceResultStatus.ConcurrencyConflict => Conflict(new { message = result.Message, status = result.Status.ToString() }),
                AttendanceResultStatus.NotFound => NotFound(new { message = result.Message }),
                AttendanceResultStatus.MeetingClosed => BadRequest(new { message = result.Message }),
                _ => BadRequest(new { message = result.Message })
            };
        }

        /// <summary>
        /// مسح كود الـ QR للشاب وتسجيل حضوره فوراً (متاح لأي خادم مسح أي شاب من أي دفعة)
        /// </summary>
        [HttpPost("scan-qr")]
        public async Task<IActionResult> ScanQr([FromBody] ScanQrAttendanceRequest request, CancellationToken cancellationToken)
        {
            var result = await _attendanceService.RecordAttendanceByQrAsync(request, cancellationToken);

            return result.Status switch
            {
                AttendanceResultStatus.Success => Ok(result),
                AttendanceResultStatus.AlreadyRecorded => Conflict(new { message = result.Message, status = result.Status.ToString() }),
                AttendanceResultStatus.ConcurrencyConflict => Conflict(new { message = result.Message, status = result.Status.ToString() }),
                AttendanceResultStatus.NotFound => NotFound(new { message = result.Message }),
                AttendanceResultStatus.MeetingClosed => BadRequest(new { message = result.Message }),
                _ => BadRequest(new { message = result.Message })
            };
        }

        /// <summary>
        /// إلغاء تسجيل حضور مخدوم في اجتماع
        /// </summary>
        [HttpDelete("{meetingId:int}/{studentId:int}")]
        public async Task<IActionResult> RemoveAttendance(int meetingId, int studentId, [FromQuery] int servantId, CancellationToken cancellationToken)
        {
            var result = await _attendanceService.RemoveAttendanceAsync(studentId, meetingId, servantId, cancellationToken);

            return result.Status switch
            {
                AttendanceResultStatus.Success => Ok(result),
                AttendanceResultStatus.ConcurrencyConflict => Conflict(new { message = result.Message }),
                AttendanceResultStatus.NotFound => NotFound(new { message = result.Message }),
                _ => BadRequest(new { message = result.Message })
            };
        }

        /// <summary>
        /// جلب كشف الحضور والغياب لدفعة معينة في اجتماع محدد
        /// </summary>
        [HttpGet("meeting/{meetingId:int}/year/{academicYear:int}")]
        public async Task<IActionResult> GetMeetingAttendanceByYear(int meetingId, int academicYear, CancellationToken cancellationToken)
        {
            var list = await _attendanceService.GetMeetingAttendanceByYearAsync(meetingId, academicYear, cancellationToken);
            return Ok(list);
        }

        /// <summary>
        /// كشف الغياب المانيول: كل الشباب مقسمين على دفعات (Tabs) مع حالة حضور كل شاب والأعداد
        /// </summary>
        [HttpGet("meeting/{meetingId:int}/manual-sheet")]
        public async Task<IActionResult> GetManualAttendanceSheet(int meetingId, CancellationToken cancellationToken)
        {
            try
            {
                var sheet = await _attendanceService.GetManualAttendanceSheetAsync(meetingId, cancellationToken);
                return Ok(sheet);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// بحث في الشباب بالاسم أو رقم الموبايل مع حالة حضورهم في اجتماع محدد (للغياب المانيول)
        /// </summary>
        [HttpGet("meeting/{meetingId:int}/search")]
        public async Task<IActionResult> SearchStudents(int meetingId, [FromQuery] string q, CancellationToken cancellationToken)
        {
            var results = await _attendanceService.SearchStudentsAsync(meetingId, q, cancellationToken);
            return Ok(results);
        }

        /// <summary>
        /// استخراج تقرير حضور وغياب شامل لفترة زمنية محددة
        /// (لخادم الفصل: يقتصر تلقائياً على فصله المسند إليه فقط - لأمين الخدمة: يشمل كل الفصول أو فصلاً محدداً مع فلتر النوع)
        /// </summary>
        [HttpPost("report")]
        public async Task<IActionResult> GenerateReport([FromBody] AttendanceReportRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var report = await _attendanceService.GenerateAttendanceReportAsync(request, cancellationToken);
                return Ok(report);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
