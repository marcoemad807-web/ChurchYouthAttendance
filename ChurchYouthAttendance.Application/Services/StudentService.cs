using ChurchYouthAttendance.Application.Common.Interfaces;
using ChurchYouthAttendance.Application.DTOs.Student;
using ChurchYouthAttendance.Application.Interfaces;
using ChurchYouthAttendance.Domain.Entities;
using ChurchYouthAttendance.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ChurchYouthAttendance.Application.Services
{
    public class StudentService : IStudentService
    {
        private readonly IAppDbContext _context;

        public StudentService(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<List<StudentDto>> GetStudentsByYearAsync(int academicYear, CancellationToken cancellationToken = default)
        {
            var students = await _context.Students
                .AsNoTracking()
                .Where(s => s.AcademicYear == academicYear)
                .OrderBy(s => s.FullName)
                .ToListAsync(cancellationToken);

            return students.Select(MapToDto).ToList();
        }

        public async Task<List<ClassGroupDto>> GetAllClassesAsync(CancellationToken cancellationToken = default)
        {
            var allStudents = await _context.Students
                .AsNoTracking()
                .OrderBy(s => s.AcademicYear)
                .ThenBy(s => s.FullName)
                .ToListAsync(cancellationToken);

            var yearLabels = new Dictionary<int, string>
            {
                { 1, "سنة أولى" },
                { 2, "سنة تانية" },
                { 3, "سنة تالتة" },
                { 4, "سنة رابعة" },
                { 5, "سنة خامسة" },
                { 6, "خريجين" }
            };

            return allStudents
                .GroupBy(s => s.AcademicYear)
                .OrderBy(g => g.Key)
                .Select(g =>
                {
                    var studentDtos = g.Select(MapToDto).ToList();
                    return new ClassGroupDto
                    {
                        AcademicYear = g.Key,
                        YearLabel = yearLabels.TryGetValue(g.Key, out var label) ? label : $"سنة {g.Key}",
                        TotalStudents = studentDtos.Count,
                        RegisteredInAppCount = studentDtos.Count(s => s.IsAppRegistered),
                        Students = studentDtos
                    };
                }).ToList();
        }

        public async Task<StudentDto?> GetStudentByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var student = await _context.Students
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

            return student == null ? null : MapToDto(student);
        }

        public async Task<StudentDto> CreateStudentAsync(CreateStudentRequest request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.FullName))
                throw new ArgumentException("اسم الشاب مطلوب.", nameof(request.FullName));

            if (string.IsNullOrWhiteSpace(request.Username))
                throw new ArgumentException("اسم المستخدم باللغة الإنجليزية مطلوب.", nameof(request.Username));

            if (string.IsNullOrWhiteSpace(request.College))
                throw new ArgumentException("اسم الكلية مطلوب.", nameof(request.College));

            if (string.IsNullOrWhiteSpace(request.FatherOfConfession))
                throw new ArgumentException("اسم أب الاعتراف مطلوب.", nameof(request.FatherOfConfession));

            if (!PhoneNumber.TryCreate(request.PhoneNumber, out var phoneVo, out var err))
                throw new ArgumentException(err ?? "رقم الموبايل غير صالح.", nameof(request.PhoneNumber));

            var cleanUsername = request.Username.Trim().ToLower();

            // التحقق من تفرد اسم المستخدم عبر النظام
            var usernameTaken = await _context.Students.AnyAsync(s => s.Username.ToLower() == cleanUsername, cancellationToken)
                || await _context.Servants.AnyAsync(s => s.Username.ToLower() == cleanUsername, cancellationToken);
            if (usernameTaken)
                throw new InvalidOperationException("اسم المستخدم محجوز بالفعل، يرجى اختيار اسم آخر.");

            // التأكد من عدم تكرار رقم الموبايل
            var cleanPhone = phoneVo!.Value;
            var exists = await _context.Students
                .AnyAsync(s => EF.Functions.Like((string)(object)s.PhoneNumber, $"%{cleanPhone}%"), cancellationToken);

            if (exists)
                throw new InvalidOperationException("رقم الموبايل مسجل بالفعل لشاب آخر.");

            var student = new Student(
                request.FullName.Trim(),
                cleanUsername,
                phoneVo,
                request.DateOfBirth,
                request.Gender,
                request.FatherOfConfession.Trim(),
                request.AcademicYear,
                request.College.Trim()
            );

            _context.Students.Add(student);
            await _context.SaveChangesAsync(cancellationToken);

            return MapToDto(student);
        }

        public async Task<StudentDto> UpdateStudentAsync(int id, UpdateStudentRequest request, CancellationToken cancellationToken = default)
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

            if (student == null)
                throw new KeyNotFoundException("الشاب غير موجود.");

            if (string.IsNullOrWhiteSpace(request.FullName))
                throw new ArgumentException("اسم الشاب مطلوب.", nameof(request.FullName));

            if (string.IsNullOrWhiteSpace(request.Username))
                throw new ArgumentException("اسم المستخدم باللغة الإنجليزية مطلوب.", nameof(request.Username));

            if (string.IsNullOrWhiteSpace(request.College))
                throw new ArgumentException("اسم الكلية مطلوب.", nameof(request.College));

            if (string.IsNullOrWhiteSpace(request.FatherOfConfession))
                throw new ArgumentException("اسم أب الاعتراف مطلوب.", nameof(request.FatherOfConfession));

            if (!PhoneNumber.TryCreate(request.PhoneNumber, out var phoneVo, out var err))
                throw new ArgumentException(err ?? "رقم الموبايل غير صالح.", nameof(request.PhoneNumber));

            var cleanUsername = request.Username.Trim().ToLower();

            // التأكد من عدم تكرار اسم المستخدم مع أي مستخدم آخر
            var usernameTaken = await _context.Students.AnyAsync(s => s.Id != id && s.Username.ToLower() == cleanUsername, cancellationToken)
                || await _context.Servants.AnyAsync(s => s.Username.ToLower() == cleanUsername, cancellationToken);
            if (usernameTaken)
                throw new InvalidOperationException("اسم المستخدم محجوز بالفعل، يرجى اختيار اسم آخر.");

            // التأكد من عدم تكرار رقم الموبايل مع شاب آخر
            var cleanPhone = phoneVo!.Value;
            var phoneTaken = await _context.Students
                .AnyAsync(s => s.Id != id && EF.Functions.Like((string)(object)s.PhoneNumber, $"%{cleanPhone}%"), cancellationToken);
            if (phoneTaken)
                throw new InvalidOperationException("رقم الموبايل مسجل بالفعل لشاب آخر.");

            student.FullName = request.FullName.Trim();
            student.Username = cleanUsername;
            student.PhoneNumber = phoneVo!;
            student.DateOfBirth = request.DateOfBirth;
            student.Gender = request.Gender;
            student.FatherOfConfession = request.FatherOfConfession.Trim();
            student.AcademicYear = request.AcademicYear;
            student.College = request.College.Trim();

            await _context.SaveChangesAsync(cancellationToken);

            return MapToDto(student);
        }

        public async Task<bool> DeleteStudentAsync(int id, CancellationToken cancellationToken = default)
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

            if (student == null)
                return false;

            _context.Students.Remove(student);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<string?> GetQrTokenAsync(int studentId, CancellationToken cancellationToken = default)
        {
            var student = await _context.Students
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == studentId, cancellationToken);

            return student?.QrToken;
        }

        public async Task<string> RegenerateQrTokenAsync(int studentId, CancellationToken cancellationToken = default)
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == studentId, cancellationToken);

            if (student == null)
                throw new KeyNotFoundException("الشاب غير موجود.");

            student.RegenerateQrToken();
            await _context.SaveChangesAsync(cancellationToken);

            return student.QrToken!;
        }

        private static StudentDto MapToDto(Student s) => new()
        {
            Id = s.Id,
            FullName = s.FullName,
            Username = s.Username,
            PhoneNumber = s.PhoneNumber.Value,
            DateOfBirth = s.DateOfBirth,
            Gender = s.Gender,
            FatherOfConfession = s.FatherOfConfession,
            AcademicYear = s.AcademicYear,
            College = s.College,
            IsAppRegistered = s.IsAppRegistered,
            AppRegisteredAt = s.AppRegisteredAt,
            QrToken = s.QrToken,
            PlainPassword = s.PlainPassword,
            CreatedAt = s.CreatedAt
        };
    }
}
