using ChurchYouthAttendance.Application.DTOs.Meeting;
using ChurchYouthAttendance.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ChurchYouthAttendance.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MeetingsController : ControllerBase
    {
        private readonly IMeetingService _meetingService;

        public MeetingsController(IMeetingService meetingService)
        {
            _meetingService = meetingService;
        }

        /// <summary>
        /// جلب كل الاجتماعات مع إجمالي عدد الحضور في كل اجتماع
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllMeetings(CancellationToken cancellationToken)
        {
            var meetings = await _meetingService.GetAllMeetingsAsync(cancellationToken);
            return Ok(meetings);
        }

        /// <summary>
        /// جلب الاجتماع المفتوح حالياً لأخذ الغياب
        /// </summary>
        [HttpGet("current-open")]
        public async Task<IActionResult> GetCurrentOpenMeeting(CancellationToken cancellationToken)
        {
            var meeting = await _meetingService.GetCurrentOpenMeetingAsync(cancellationToken);
            if (meeting == null)
                return NotFound(new { message = "لا يوجد اجتماع مفتوح حالياً لتسجيل الحضور." });

            return Ok(meeting);
        }

        /// <summary>
        /// إنشاء اجتماع جديد (بواسطة أمين الخدمة أو الخادم)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateMeeting([FromBody] CreateMeetingRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var meeting = await _meetingService.CreateMeetingAsync(request, cancellationToken);
                return Ok(meeting);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        /// <summary>
        /// فتح أو قفل تسجيل الغياب للاجتماع
        /// </summary>
        [HttpPut("{id:int}/toggle-status")]
        public async Task<IActionResult> ToggleMeetingStatus(int id, CancellationToken cancellationToken)
        {
            try
            {
                var meeting = await _meetingService.ToggleMeetingStatusAsync(id, cancellationToken);
                var statusText = meeting.IsOpen ? "تم فتح تسجيل الحضور للاجتماع بنجاح." : "تم قفل تسجيل الحضور للاجتماع.";
                return Ok(new
                {
                    message = statusText,
                    data = meeting
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// جلب اجتماعات أيام الجمعة فقط
        /// </summary>
        [HttpGet("fridays")]
        public async Task<IActionResult> GetFridayMeetings(CancellationToken cancellationToken)
        {
            var meetings = await _meetingService.GetFridayMeetingsAsync(cancellationToken);
            return Ok(meetings);
        }

        /// <summary>
        /// جلب خيارات أيام الجمعة لفلتر واجهة الخدام (لعرض أيام الجمعة السابقة والحالية مع حالة كل اجتماع)
        /// </summary>
        [HttpGet("friday-options")]
        public async Task<IActionResult> GetFridayOptions([FromQuery] int count = 12, [FromQuery] int futureCount = 4, CancellationToken cancellationToken = default)
        {
            var options = await _meetingService.GetFridayOptionsAsync(count, futureCount, cancellationToken);
            return Ok(options);
        }

        /// <summary>
        /// جلب أو إنشاء اجتماع تلقائياً ليوم جمعة محدد عند اختياره من الفلتر
        /// </summary>
        [HttpPost("get-or-create-by-date")]
        public async Task<IActionResult> GetOrCreateMeetingByDate([FromQuery] DateOnly date, CancellationToken cancellationToken)
        {
            var meeting = await _meetingService.GetOrCreateMeetingByDateAsync(date, cancellationToken);
            return Ok(meeting);
        }
    }
}
