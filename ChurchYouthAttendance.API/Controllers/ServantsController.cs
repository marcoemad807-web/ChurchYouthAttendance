using ChurchYouthAttendance.Application.DTOs.Servant;
using ChurchYouthAttendance.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ChurchYouthAttendance.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServantsController : ControllerBase
    {
        private readonly IServantService _servantService;

        public ServantsController(IServantService servantService)
        {
            _servantService = servantService;
        }

        /// <summary>
        /// جلب جميع الخدام مع الدفعات المسندة إليهم (خاص بأمين الخدمة)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllServants(CancellationToken cancellationToken)
        {
            var servants = await _servantService.GetAllServantsAsync(cancellationToken);
            return Ok(servants);
        }

        /// <summary>
        /// جلب بيانات خادم بالـ Id
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetServantById(int id, CancellationToken cancellationToken)
        {
            var servant = await _servantService.GetServantByIdAsync(id, cancellationToken);
            if (servant == null)
                return NotFound(new { message = "الخادم غير موجود." });

            return Ok(servant);
        }

        /// <summary>
        /// إضافة خادم جديد وتحديد دفعته المسندة وكلمة مروره (خاص بأمين الخدمة)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateServant([FromBody] CreateServantRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var servant = await _servantService.CreateServantAsync(request, cancellationToken);
                return CreatedAtAction(nameof(GetServantById), new { id = servant.Id }, servant);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        /// <summary>
        /// تعديل بيانات خادم أو تغيير دفعته المسندة (خاص بأمين الخدمة)
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateServant(int id, [FromBody] UpdateServantRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var servant = await _servantService.UpdateServantAsync(id, request, cancellationToken);
                return Ok(servant);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// حذف خادم (خاص بأمين الخدمة)
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteServant(int id, CancellationToken cancellationToken)
        {
            var deleted = await _servantService.DeleteServantAsync(id, cancellationToken);
            if (!deleted)
                return NotFound(new { message = "الخادم غير موجود." });

            return Ok(new { message = "تم حذف الخادم بنجاح." });
        }
    }
}
