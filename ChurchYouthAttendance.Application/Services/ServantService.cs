using ChurchYouthAttendance.Application.Common.Interfaces;
using ChurchYouthAttendance.Application.Common.Security;
using ChurchYouthAttendance.Application.DTOs.Servant;
using ChurchYouthAttendance.Application.Interfaces;
using ChurchYouthAttendance.Domain.Entities;
using ChurchYouthAttendance.Domain.Enum;
using ChurchYouthAttendance.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ChurchYouthAttendance.Application.Services
{
    public class ServantService : IServantService
    {
        private readonly IAppDbContext _context;
        private readonly IPasswordHasher _passwordHasher;

        public ServantService(IAppDbContext context, IPasswordHasher passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<List<ServantDto>> GetAllServantsAsync(CancellationToken cancellationToken = default)
        {
            var servants = await _context.Servants
                .AsNoTracking()
                .OrderBy(s => s.Role)
                .ThenBy(s => s.AssignedAcademicYear)
                .ThenBy(s => s.FullName)
                .ToListAsync(cancellationToken);

            return servants.Select(MapToDto).ToList();
        }

        public async Task<ServantDto?> GetServantByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var servant = await _context.Servants
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

            return servant == null ? null : MapToDto(servant);
        }

        public async Task<ServantDto> CreateServantAsync(CreateServantRequest request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.FullName))
                throw new ArgumentException("اسم الخادم مطلوب.", nameof(request.FullName));

            if (string.IsNullOrWhiteSpace(request.Username))
                throw new ArgumentException("اسم المستخدم باللغة الإنجليزية مطلوب.", nameof(request.Username));

            if (string.IsNullOrWhiteSpace(request.FatherOfConfession))
                throw new ArgumentException("اسم أب الاعتراف مطلوب.", nameof(request.FatherOfConfession));

            if (!PhoneNumber.TryCreate(request.PhoneNumber, out var phoneVo, out var err))
                throw new ArgumentException(err ?? "رقم الموبايل غير صحيح.", nameof(request.PhoneNumber));

            if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
                throw new ArgumentException("كلمة المرور يجب ألا تقل عن 6 أحرف.", nameof(request.Password));

            var cleanUsername = request.Username.Trim().ToLower();

            // فحص عدم تكرار اسم المستخدم
            var usernameTaken = await _context.Servants.AnyAsync(s => s.Username.ToLower() == cleanUsername, cancellationToken)
                || await _context.Students.AnyAsync(s => s.Username.ToLower() == cleanUsername, cancellationToken);
            if (usernameTaken)
                throw new InvalidOperationException("اسم المستخدم مسجل بالفعل، يرجى اختيار اسم آخر.");

            var cleanPhone = phoneVo!.Value;
            var exists = await _context.Servants
                .AnyAsync(s => EF.Functions.Like((string)(object)s.PhoneNumber, $"%{cleanPhone}%"), cancellationToken);

            if (exists)
                throw new InvalidOperationException("رقم الموبايل مسجل بالفعل لخادم آخر.");

            var passwordHash = _passwordHasher.HashPassword(request.Password);

            var years = ExtractYears(request.AssignedAcademicYears, request.AssignedYears, request.AssignedAcademicYear);
            if (request.Role == UserRole.Servant && years.Count == 0)
            {
                throw new ArgumentException("يجب تحديد دفعة واحدة على الأقل مسندة للخادم.", nameof(request.AssignedAcademicYears));
            }

            var yearsString = years.Count > 0 ? string.Join(",", years) : null;
            var primaryYear = years.Count > 0 ? years[0] : (int?)null;

            var servant = new Servant(
                request.FullName.Trim(),
                cleanUsername,
                phoneVo,
                passwordHash,
                request.DateOfBirth,
                request.Gender,
                request.FatherOfConfession.Trim(),
                request.Role,
                request.Role == UserRole.Admin ? null : primaryYear,
                request.Role == UserRole.Admin ? 0 : request.ClassGender,
                request.Role == UserRole.Admin ? null : yearsString
            );

            servant.PlainPassword = request.Password;
            _context.Servants.Add(servant);
            await _context.SaveChangesAsync(cancellationToken);

            return MapToDto(servant);
        }

        public async Task<ServantDto> UpdateServantAsync(int id, UpdateServantRequest request, CancellationToken cancellationToken = default)
        {
            var servant = await _context.Servants
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

            if (servant == null)
                throw new KeyNotFoundException("الخادم غير موجود.");

            if (string.IsNullOrWhiteSpace(request.FullName))
                throw new ArgumentException("اسم الخادم مطلوب.", nameof(request.FullName));

            if (string.IsNullOrWhiteSpace(request.Username))
                throw new ArgumentException("اسم المستخدم باللغة الإنجليزية مطلوب.", nameof(request.Username));

            if (string.IsNullOrWhiteSpace(request.FatherOfConfession))
                throw new ArgumentException("اسم أب الاعتراف مطلوب.", nameof(request.FatherOfConfession));

            if (!PhoneNumber.TryCreate(request.PhoneNumber, out var phoneVo, out var err))
                throw new ArgumentException(err ?? "رقم الموبايل غير صحيح.", nameof(request.PhoneNumber));

            var cleanUsername = request.Username.Trim().ToLower();

            var usernameTaken = await _context.Servants.AnyAsync(s => s.Id != id && s.Username.ToLower() == cleanUsername, cancellationToken)
                || await _context.Students.AnyAsync(s => s.Id != id && s.Username.ToLower() == cleanUsername, cancellationToken);
            if (usernameTaken)
                throw new InvalidOperationException("اسم المستخدم مسجل بالفعل، يرجى اختيار اسم آخر.");

            var cleanPhone = phoneVo!.Value;
            var phoneTaken = await _context.Servants
                .AnyAsync(s => s.Id != id && EF.Functions.Like((string)(object)s.PhoneNumber, $"%{cleanPhone}%"), cancellationToken);
            if (phoneTaken)
                throw new InvalidOperationException("رقم الموبايل مسجل بالفعل لخادم آخر.");

            servant.FullName = request.FullName.Trim();
            servant.Username = cleanUsername;
            servant.PhoneNumber = phoneVo!;
            servant.DateOfBirth = request.DateOfBirth;
            servant.Gender = request.Gender;
            servant.FatherOfConfession = request.FatherOfConfession.Trim();
            servant.Role = request.Role;

            if (request.Role == UserRole.Admin)
            {
                servant.SetAssignedClasses(null, 0);
            }
            else
            {
                var years = ExtractYears(request.AssignedAcademicYears, request.AssignedYears, request.AssignedAcademicYear);
                servant.SetAssignedClasses(years, request.ClassGender);
            }

            await _context.SaveChangesAsync(cancellationToken);

            return MapToDto(servant);
        }

        public async Task<bool> DeleteServantAsync(int id, CancellationToken cancellationToken = default)
        {
            var servant = await _context.Servants
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

            if (servant == null)
                return false;

            _context.Servants.Remove(servant);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        private static List<int> ExtractYears(List<int>? yearsList, string? yearsString, int? singleYear)
        {
            var result = yearsList?.Where(y => y > 0).Distinct().OrderBy(y => y).ToList() ?? new List<int>();
            if (result.Count == 0 && !string.IsNullOrWhiteSpace(yearsString))
            {
                result = Servant.ParseAcademicYears(yearsString);
            }
            if (result.Count == 0 && singleYear.HasValue && singleYear.Value > 0)
            {
                result.Add(singleYear.Value);
            }
            return result;
        }

        private static ServantDto MapToDto(Servant s) => new()
        {
            Id = s.Id,
            FullName = s.FullName,
            Username = s.Username,
            PhoneNumber = s.PhoneNumber.Value,
            DateOfBirth = s.DateOfBirth,
            Gender = s.Gender,
            FatherOfConfession = s.FatherOfConfession,
            Role = s.Role,
            AssignedAcademicYear = s.AssignedAcademicYear,
            AssignedAcademicYears = s.GetAssignedAcademicYearsList(),
            ClassGender = s.ClassGender,
            PlainPassword = s.PlainPassword,
            CreatedAt = s.CreatedAt
        };
    }
}
