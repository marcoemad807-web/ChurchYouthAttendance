using ChurchYouthAttendance.Application.DTOs.Student;
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
    public class StudentsController : ControllerBase
    {
        private readonly IStudentService _studentService;

        public StudentsController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        /// <summary>
        /// Ø¬Ù„Ø¨ Ø´Ø¨Ø§Ø¨ ÙØµÙ„ Ø§Ù„Ø®Ø§Ø¯Ù… (Ø£ÙˆÙ„ Ø´Ø§Ø´Ø© ØªØ¸Ù‡Ø± Ù„Ù„Ø®Ø§Ø¯Ù… Ù…Ø¹ Ø¹Ù„Ø§Ù…Ø© ØµØ­ Ù„Ù…Ù† Ø³Ø¬Ù„ ÙÙŠ Ø§Ù„ØªØ·Ø¨ÙŠÙ‚)
        /// </summary>
        /// <summary>
        /// جلب جميع الشباب في الخدمة (أو تصفيتهم بالدفعة)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllStudents([FromQuery] int? academicYear, CancellationToken cancellationToken)
        {
            if (academicYear.HasValue && academicYear.Value > 0)
            {
                var students = await _studentService.GetStudentsByYearAsync(academicYear.Value, cancellationToken);
                return Ok(students);
            }
            var classes = await _studentService.GetAllClassesAsync(cancellationToken);
            var all = new List<StudentDto>();
            foreach (var c in classes)
            {
                all.AddRange(c.Students);
            }
            return Ok(all);
        }

        [HttpGet("my-class")]
        public async Task<IActionResult> GetMyClass([FromQuery] int academicYear, CancellationToken cancellationToken)
        {
            var students = await _studentService.GetStudentsByYearAsync(academicYear, cancellationToken);
            return Ok(students);
        }

        /// <summary>
        /// Ø¬Ù„Ø¨ Ø¬Ù…ÙŠØ¹ Ø§Ù„ÙØµÙˆÙ„ Ù…Ù‚Ø³Ù…Ø© Ø¨Ø§Ù„Ø¯ÙØ¹Ø§Øª Ù…Ø¹ Ø£Ø¹Ø¯Ø§Ø¯ Ø§Ù„Ù…Ø³Ø¬Ù„ÙŠÙ† (Ø®Ø§Øµ Ø¨Ø£Ù…ÙŠÙ† Ø§Ù„Ø®Ø¯Ù…Ø©)
        /// </summary>
        [HttpGet("all-classes")]
        public async Task<IActionResult> GetAllClasses(CancellationToken cancellationToken)
        {
            var classes = await _studentService.GetAllClassesAsync(cancellationToken);
            return Ok(classes);
        }

        /// <summary>
        /// Ø¬Ù„Ø¨ Ø¨ÙŠØ§Ù†Ø§Øª Ø´Ø§Ø¨ Ù…Ø¹ÙŠÙ† Ø¨Ø§Ù„Ù€ Id
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetStudentById(int id, CancellationToken cancellationToken)
        {
            var student = await _studentService.GetStudentByIdAsync(id, cancellationToken);
            if (student == null)
                return NotFound(new { message = "Ø§Ù„Ø´Ø§Ø¨ ØºÙŠØ± Ù…ÙˆØ¬ÙˆØ¯." });

            return Ok(student);
        }

        /// <summary>
        /// Ø¥Ø¶Ø§ÙØ© Ø´Ø§Ø¨ Ø¬Ø¯ÙŠØ¯ ÙÙŠ ÙƒØ´ÙˆÙØ§Øª Ø§Ù„ÙƒÙ†ÙŠØ³Ø©
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateStudent([FromBody] CreateStudentRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var student = await _studentService.CreateStudentAsync(request, cancellationToken);
                return CreatedAtAction(nameof(GetStudentById), new { id = student.Id }, student);
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
        /// ØªØ¹Ø¯ÙŠÙ„ Ø¨ÙŠØ§Ù†Ø§Øª Ø´Ø§Ø¨
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateStudent(int id, [FromBody] UpdateStudentRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var student = await _studentService.UpdateStudentAsync(id, request, cancellationToken);
                return Ok(student);
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
        /// Ø­Ø°Ù Ø´Ø§Ø¨
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteStudent(int id, CancellationToken cancellationToken)
        {
            var deleted = await _studentService.DeleteStudentAsync(id, cancellationToken);
            if (!deleted)
                return NotFound(new { message = "Ø§Ù„Ø´Ø§Ø¨ ØºÙŠØ± Ù…ÙˆØ¬ÙˆØ¯." });

            return Ok(new { message = "ØªÙ… Ø­Ø°Ù Ø§Ù„Ø´Ø§Ø¨ Ø¨Ù†Ø¬Ø§Ø­." });
        }

        /// <summary>
        /// Ø¬Ù„Ø¨ ÙƒÙˆØ¯ Ø§Ù„Ù€ QR Ø§Ù„Ø®Ø§Øµ Ø¨Ø§Ù„Ø´Ø§Ø¨ Ù„Ø¹Ø±Ø¶Ù‡ Ø¹Ù„Ù‰ Ø´Ø§Ø´Ø© Ø§Ù„Ù…ÙˆØ¨Ø§ÙŠÙ„
        /// </summary>
        [HttpGet("{id:int}/qr")]
        public async Task<IActionResult> GetQr(int id, CancellationToken cancellationToken)
        {
            var qr = await _studentService.GetQrTokenAsync(id, cancellationToken);
            if (qr == null)
                return NotFound(new { message = "Ù„Ù… ÙŠØªÙ… ØªÙˆÙ„ÙŠØ¯ ÙƒÙˆØ¯ QR Ù„Ù‡Ø°Ø§ Ø§Ù„Ø´Ø§Ø¨ØŒ ÙŠØ¬Ø¨ ØªÙØ¹ÙŠÙ„ Ø­Ø³Ø§Ø¨Ù‡ Ø£ÙˆÙ„Ø§Ù‹." });

            return Ok(new { studentId = id, qrToken = qr });
        }

        /// <summary>
        /// ØªØ¬Ø¯ÙŠØ¯ ÙƒÙˆØ¯ Ø§Ù„Ù€ QR Ù„Ù„Ø´Ø§Ø¨ ÙÙŠ Ø­Ø§Ù„Ø© Ø§Ù„Ø´Ùƒ Ø¨ØªØ³Ø±ÙŠØ¨Ù‡
        /// </summary>
        [HttpPost("{id:int}/regenerate-qr")]
        public async Task<IActionResult> RegenerateQr(int id, CancellationToken cancellationToken)
        {
            try
            {
                var newQr = await _studentService.RegenerateQrTokenAsync(id, cancellationToken);
                return Ok(new { message = "ØªÙ… ØªØ¬Ø¯ÙŠØ¯ ÙƒÙˆØ¯ Ø§Ù„Ù€ QR Ø¨Ù†Ø¬Ø§Ø­.", qrToken = newQr });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}

