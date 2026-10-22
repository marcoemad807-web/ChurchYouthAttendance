using ChurchYouthAttendance.Application.DTOs.FollowUp;
using ChurchYouthAttendance.Application.Interfaces;
using ChurchYouthAttendance.Domain.Enum;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ChurchYouthAttendance.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FollowUpController : ControllerBase
    {
        private readonly IFollowUpService _followUpService;

        public FollowUpController(IFollowUpService followUpService)
        {
            _followUpService = followUpService;
        }

        /// <summary>
        /// جلب كشف الافتقاد للشباب الغائبين لعدد معين من الاجتماعات المتتالية (لأمين الخدمة مع فلترة الدفعة أو النوع)
        /// </summary>
        [HttpGet("missing")]
        public async Task<IActionResult> GetMissingStudents(
            [FromQuery] int? academicYear,
            [FromQuery] UserGender? gender,
            [FromQuery] int minConsecutiveAbsences = 2,
            CancellationToken cancellationToken = default)
        {
            var list = await _followUpService.GetMissingStudentsAsync(academicYear, gender, minConsecutiveAbsences, cancellationToken: cancellationToken);
            return Ok(list);
        }

        /// <summary>
        /// جلب كشف الافتقاد الخاص بدفعة خادم معين بناءً على دفعته والنوع المسند إليه
        /// </summary>
        [HttpGet("servant/{servantId:int}")]
        public async Task<IActionResult> GetMissingStudentsForServant(
            int servantId,
            [FromQuery] int minConsecutiveAbsences = 2,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var list = await _followUpService.GetMissingStudentsForServantAsync(servantId, minConsecutiveAbsences, cancellationToken);
                return Ok(list);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
