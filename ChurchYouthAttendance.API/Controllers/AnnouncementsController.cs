using ChurchYouthAttendance.Application.DTOs.Announcement;
using ChurchYouthAttendance.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ChurchYouthAttendance.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnnouncementsController : ControllerBase
    {
        private readonly IAnnouncementService _announcementService;

        public AnnouncementsController(IAnnouncementService announcementService)
        {
            _announcementService = announcementService;
        }

        /// <summary>
        /// إرسال إعلان جديد من أمين الخدمة للشباب (مع إرسال Push Notification وتسميع لحظي عبر SignalR)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateAnnouncement([FromBody] CreateAnnouncementRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _announcementService.CreateAnnouncementAsync(request, cancellationToken);
                return Ok(new
                {
                    message = "تم نشر الإعلان وإرسال الإشعارات للشباب بنجاح.",
                    data = result
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// جلب الإعلانات الخاصة بشاب معين حسب دفعته الدراسية (يشمل الإعلانات العامة وإعلانات دفعته)
        /// </summary>
        [HttpGet("feed")]
        public async Task<IActionResult> GetStudentFeed([FromQuery] int academicYear, [FromQuery] int limit = 20, CancellationToken cancellationToken = default)
        {
            var announcements = await _announcementService.GetAnnouncementsForStudentAsync(academicYear, limit, cancellationToken);
            return Ok(announcements);
        }

        /// <summary>
        /// جلب كل الإعلانات السابقة (لأمناء الخدمة والخدام)
        /// </summary>
        [HttpGet("history")]
        public async Task<IActionResult> GetAllAnnouncements([FromQuery] int limit = 50, CancellationToken cancellationToken = default)
        {
            var announcements = await _announcementService.GetAllAnnouncementsAsync(limit, cancellationToken);
            return Ok(announcements);
        }

        /// <summary>
        /// تحديث رمز جهاز الشاب (FCM Token) عند تسجيل دخوله من تطبيق الموبايل لاستقبال الإشعارات
        /// </summary>
        [HttpPut("device-token")]
        public async Task<IActionResult> UpdateDeviceToken([FromBody] UpdateDeviceTokenRequest request, CancellationToken cancellationToken)
        {
            var success = await _announcementService.UpdateStudentDeviceTokenAsync(request, cancellationToken);
            if (!success)
            {
                return NotFound(new { message = "الشاب غير موجود لتحديث رمز جهازه." });
            }

            return Ok(new { message = "تم تسجيل رمز الجهاز لاستقبال الإشعارات بنجاح." });
        }
    }
}
