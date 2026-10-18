using ChurchYouthAttendance.Application.DTOs.Auth;
using ChurchYouthAttendance.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ChurchYouthAttendance.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// ØªØ³Ø¬ÙŠÙ„ Ø¯Ø®ÙˆÙ„ Ø§Ù„Ù…Ø³ØªØ®Ø¯Ù… (Ø´Ø§Ø¨ØŒ Ø®Ø§Ø¯Ù…ØŒ Ø£Ùˆ Ø£Ù…ÙŠÙ† Ø®Ø¯Ù…Ø©) Ø¨Ø±Ù‚Ù… Ø§Ù„Ù…ÙˆØ¨Ø§ÙŠÙ„ ÙˆÙƒÙ„Ù…Ø© Ø§Ù„Ø³Ø±
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _authService.LoginAsync(request, cancellationToken);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Ø§Ø³ØªØ±Ø¬Ø§Ø¹ Ø§Ù‚ØªØ±Ø§Ø­Ø§Øª Ø£Ø³Ù…Ø§Ø¡ Ø§Ù„Ø´Ø¨Ø§Ø¨ ØºÙŠØ± Ø§Ù„Ù…Ø³Ø¬Ù„ÙŠÙ† Ø¨Ø§Ù„ØªØ·Ø¨ÙŠÙ‚ Ø¨Ù†Ø§Ø¡Ù‹ Ø¹Ù„Ù‰ Ø§Ù„Ø³Ù†Ø© Ø§Ù„Ø¯Ø±Ø§Ø³ÙŠØ© ÙˆØ§Ù„Ù†ÙˆØ¹ Ù„ÙÙ„ØªØ±Ø© Ø§Ù„Ø§Ø³Ù… ÙˆØ§Ù„Ù…Ù„Ø¡ Ø§Ù„ØªÙ„Ù‚Ø§Ø¦ÙŠ Ù„Ù„Ø¨ÙŠØ§Ù†Ø§Øª
        /// </summary>
        [HttpGet("lookup-students")]
        public async Task<IActionResult> LookupStudents(
            [FromQuery] int academicYear,
            [FromQuery] ChurchYouthAttendance.Domain.Enum.UserGender gender,
            [FromQuery] string? query,
            CancellationToken cancellationToken)
        {
            if (academicYear <= 0)
                return BadRequest(new { message = "Ø§Ù„Ø³Ù†Ø© Ø§Ù„Ø¯Ø±Ø§Ø³ÙŠØ© Ù…Ø·Ù„ÙˆØ¨Ø© ÙˆÙŠØ¬Ø¨ Ø£Ù† ØªÙƒÙˆÙ† Ø±Ù‚Ù…Ø§Ù‹ Ù…ÙˆØ¬Ø¨Ø§Ù‹." });

            var students = await _authService.GetUnregisteredStudentsLookupAsync(academicYear, gender, query, cancellationToken);
            return Ok(students);
        }

        /// <summary>
        /// ØªØ³Ø¬ÙŠÙ„ Ø§Ù„Ø´Ø§Ø¨ Ù„Ù†ÙØ³Ù‡ ÙÙŠ Ø§Ù„ØªØ·Ø¨ÙŠÙ‚ Ù„Ø£ÙˆÙ„ Ù…Ø±Ø© (ØªÙØ¹ÙŠÙ„ Ø§Ù„Ø­Ø³Ø§Ø¨ØŒ ØªÙˆÙ„ÙŠØ¯ ÙƒÙˆØ¯ Ø§Ù„Ù€ QRØŒ ÙˆØªÙØ¹ÙŠÙ„ Ø¹Ù„Ø§Ù…Ø© Ø§Ù„ØµØ­ Ù„Ù„Ø®Ø§Ø¯Ù…)
        /// </summary>
        [HttpPost("register-student")]
        public async Task<IActionResult> RegisterStudent([FromBody] StudentSelfRegisterRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _authService.RegisterStudentAsync(request, cancellationToken);
                return Ok(new
                {
                    message = "ØªÙ… ØªØ³Ø¬ÙŠÙ„ ÙˆØªÙØ¹ÙŠÙ„ Ø­Ø³Ø§Ø¨Ùƒ Ø¨Ù†Ø¬Ø§Ø­! ÙƒÙˆØ¯ Ø§Ù„Ù€ QR Ø¬Ø§Ù‡Ø² Ø§Ù„Ø¢Ù†.",
                    data = response
                });
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
                return Conflict(new { message = ex.Message });
            }
        }

        /// <summary>
        /// ØªØºÙŠÙŠØ± ÙƒÙ„Ù…Ø© Ø§Ù„Ù…Ø±ÙˆØ± Ù„Ù„Ù…Ø³ØªØ®Ø¯Ù… (Ø®Ø§Ø¯Ù…ØŒ Ø£Ù…ÙŠÙ†ØŒ Ø£Ùˆ Ø´Ø§Ø¨)
        /// </summary>
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
        {
            try
            {
                await _authService.ChangePasswordAsync(request, cancellationToken);
                return Ok(new { message = "ØªÙ… ØªØºÙŠÙŠØ± ÙƒÙ„Ù…Ø© Ø§Ù„Ù…Ø±ÙˆØ± Ø¨Ù†Ø¬Ø§Ø­." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
        /// <summary>
        /// تعيين كلمة مرور جديدة للمستخدم بواسطة أمين الخدمة (بدون الحاجة لكلمة المرور الحالية)
        /// </summary>
        [HttpPost("admin/set-password")]
        public async Task<IActionResult> AdminSetPassword([FromBody] AdminSetPasswordRequest request, CancellationToken cancellationToken)
        {
            try
            {
                await _authService.AdminSetPasswordAsync(request, cancellationToken);
                return Ok(new { message = "تم تعيين كلمة المرور الجديدة بنجاح." });
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
    }
}

