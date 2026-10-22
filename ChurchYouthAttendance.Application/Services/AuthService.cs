using ChurchYouthAttendance.Application.Common.Interfaces;
using ChurchYouthAttendance.Application.Common.Security;
using ChurchYouthAttendance.Application.DTOs.Auth;
using ChurchYouthAttendance.Application.Interfaces;
using ChurchYouthAttendance.Domain.Entities;
using ChurchYouthAttendance.Domain.Enum;
using ChurchYouthAttendance.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChurchYouthAttendance.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAppDbContext _context;
        private readonly IPasswordHasher _passwordHasher;

        public AuthService(IAppDbContext context, IPasswordHasher passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
        {
            var identifier = (request.Username ?? request.PhoneNumber ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(identifier) || string.IsNullOrWhiteSpace(request.Password))
                throw new ArgumentException("اسم المستخدم أو رقم الموبايل وكلمة المرور مطلوبان.");

            var cleanPhone = CleanPhone(identifier);
            var lowerIdentifier = identifier.ToLower();

            // 1. فحص هل المستخدم خادم أو أمين خدمة (بالـ Username الإنجليزي أو رقم الموبايل)
            var servant = await _context.Servants
                .AsNoTracking()
                .FirstOrDefaultAsync(s =>
                    (s.Username != null && s.Username.ToLower() == lowerIdentifier) ||
                    EF.Functions.Like((string)(object)s.PhoneNumber, $"%{cleanPhone}%"), cancellationToken);

            if (servant != null)
            {
                var isPasswordValid = _passwordHasher.VerifyPassword(request.Password, servant.PasswordHash ?? string.Empty);
                if (!isPasswordValid)
                    throw new UnauthorizedAccessException("اسم المستخدم أو كلمة المرور غير صحيحة.");

                return new AuthResponseDto
                {
                    UserId = servant.Id,
                    FullName = servant.FullName,
                    Username = servant.Username,
                    PhoneNumber = servant.PhoneNumber.Value,
                    Role = servant.Role,
                    AssignedAcademicYear = servant.AssignedAcademicYear,
                    AssignedAcademicYears = servant.GetAssignedAcademicYearsList(),
                    ClassGender = servant.ClassGender,
                    Token = GenerateToken(servant.Id, servant.Role.ToString(), servant.FullName)
                };
            }

            // 2. فحص هل المستخدم شاب (Student) (بالـ Username الإنجليزي أو رقم الموبايل)
            var student = await _context.Students
                .FirstOrDefaultAsync(s =>
                    (s.Username != null && s.Username.ToLower() == lowerIdentifier) ||
                    EF.Functions.Like((string)(object)s.PhoneNumber, $"%{cleanPhone}%"), cancellationToken);

            if (student != null)
            {
                if (!student.IsAppRegistered || string.IsNullOrEmpty(student.PasswordHash))
                    throw new UnauthorizedAccessException("هذا الحساب مسجل في كشوف الكنيسة ولكنه لم يفعل في التطبيق بعد. اضغط على 'تسجيل حساب جديد' بالأسفل لتفعيله.");

                var isPasswordValid = _passwordHasher.VerifyPassword(request.Password, student.PasswordHash);
                if (!isPasswordValid)
                    throw new UnauthorizedAccessException("اسم المستخدم أو كلمة المرور غير صحيحة.");

                // تحديث DeviceToken إذا تم إرساله من الموبايل لضمان وصول الإشعارات
                if (!string.IsNullOrWhiteSpace(request.DeviceToken) && student.DeviceToken != request.DeviceToken)
                {
                    student.DeviceToken = request.DeviceToken;
                    await _context.SaveChangesAsync(cancellationToken);
                }

                return new AuthResponseDto
                {
                    UserId = student.Id,
                    FullName = student.FullName,
                    Username = student.Username,
                    PhoneNumber = student.PhoneNumber.Value,
                    Role = UserRole.Student,
                    AcademicYear = student.AcademicYear,
                    QrToken = student.QrToken,
                    Token = GenerateToken(student.Id, UserRole.Student.ToString(), student.FullName)
                };
            }

            throw new UnauthorizedAccessException("اسم المستخدم أو كلمة المرور غير صحيحة.");
        }

        public async Task<List<StudentLookupDto>> GetUnregisteredStudentsLookupAsync(
            int academicYear,
            UserGender gender,
            string? query = null,
            CancellationToken cancellationToken = default)
        {
            var baseQuery = _context.Students
                .AsNoTracking()
                .Where(s => s.AcademicYear == academicYear && s.Gender == gender && !s.IsAppRegistered);

            if (!string.IsNullOrWhiteSpace(query))
            {
                var cleanQuery = query.Trim();
                baseQuery = baseQuery.Where(s => s.FullName.Contains(cleanQuery));
            }

            return await baseQuery
                .OrderBy(s => s.FullName)
                .Take(25)
                .Select(s => new StudentLookupDto
                {
                    Id = s.Id,
                    FullName = s.FullName,
                    PhoneNumber = string.Empty, // Privacy: do not expose phone in lookup
                    DateOfBirth = s.DateOfBirth,
                    Gender = s.Gender,
                    FatherOfConfession = s.FatherOfConfession,
                    AcademicYear = s.AcademicYear,
                    College = s.College
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<AuthResponseDto> RegisterStudentAsync(StudentSelfRegisterRequest request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.FullName))
                throw new ArgumentException("الاسم بالكامل مطلوب.", nameof(request.FullName));

            if (string.IsNullOrWhiteSpace(request.Username))
                throw new ArgumentException("اسم المستخدم باللغة الإنجليزية مطلوب.", nameof(request.Username));

            if (string.IsNullOrWhiteSpace(request.College))
                throw new ArgumentException("الكلية مطلوبة.", nameof(request.College));

            if (string.IsNullOrWhiteSpace(request.FatherOfConfession))
                throw new ArgumentException("أب الاعتراف مطلوب.", nameof(request.FatherOfConfession));

            if (!PhoneNumber.TryCreate(request.PhoneNumber, out var phoneVo, out var phoneError))
                throw new ArgumentException(phoneError ?? "رقم الموبايل غير صالح.", nameof(request.PhoneNumber));

            if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
                throw new ArgumentException("كلمة المرور يجب ألا تقل عن 6 أحرف أو أرقام.", nameof(request.Password));

            var cleanPhone = phoneVo!.Value;
            var cleanUsername = request.Username.Trim().ToLower();

            // فحص تفرد اسم المستخدم عبر الخدام
            var usernameTakenByServant = await _context.Servants
                .AnyAsync(s => s.Username.ToLower() == cleanUsername, cancellationToken);
            if (usernameTakenByServant)
                throw new InvalidOperationException("اسم المستخدم هذا محجوز بالفعل، يرجى اختيار اسم مستخدم آخر.");

            Student targetStudent;

            if (request.ExistingStudentId.HasValue)
            {
                // أ) اختار الشاب اسمه من اقتراحات كشوفات الكنيسة
                var existingStudent = await _context.Students
                    .FirstOrDefaultAsync(s => s.Id == request.ExistingStudentId.Value, cancellationToken);

                if (existingStudent == null)
                    throw new KeyNotFoundException("بيانات الشاب المحددة غير موجودة بالكشوفات.");

                if (existingStudent.IsAppRegistered)
                    throw new InvalidOperationException("هذا الحساب مسجل ومفعل بالفعل في التطبيق! يمكنك تسجيل الدخول مباشرة.");

                var usernameTakenByOtherStudent = await _context.Students
                    .AnyAsync(s => s.Id != existingStudent.Id && s.Username.ToLower() == cleanUsername, cancellationToken);
                if (usernameTakenByOtherStudent)
                    throw new InvalidOperationException("اسم المستخدم هذا محجوز بالفعل، يرجى اختيار اسم مستخدم آخر.");

                var phoneTakenByOtherStudent = await _context.Students
                    .AnyAsync(s => s.Id != existingStudent.Id && EF.Functions.Like((string)(object)s.PhoneNumber, $"%{cleanPhone}%"), cancellationToken);
                if (phoneTakenByOtherStudent)
                    throw new InvalidOperationException("رقم الموبايل مسجل بالفعل لشاب آخر.");

                var passwordHash = _passwordHasher.HashPassword(request.Password);
                existingStudent.CompleteAppRegistration(passwordHash);
                existingStudent.PlainPassword = request.Password;
                existingStudent.FullName = request.FullName.Trim();
                existingStudent.Username = cleanUsername;
                existingStudent.PhoneNumber = phoneVo;
                existingStudent.DateOfBirth = request.DateOfBirth;
                existingStudent.Gender = request.Gender;
                existingStudent.FatherOfConfession = request.FatherOfConfession.Trim();
                existingStudent.AcademicYear = request.AcademicYear;
                existingStudent.College = request.College.Trim();

                if (!string.IsNullOrWhiteSpace(request.DeviceToken))
                    existingStudent.DeviceToken = request.DeviceToken;

                targetStudent = existingStudent;
            }
            else
            {
                // ب) لم يختر من القائمة - فحص هل يوجد شاب غير مفعل بنفس رقم الموبايل بالكشوفات
                var existingStudentByPhone = await _context.Students
                    .FirstOrDefaultAsync(s => EF.Functions.Like((string)(object)s.PhoneNumber, $"%{cleanPhone}%"), cancellationToken);

                if (existingStudentByPhone != null)
                {
                    if (existingStudentByPhone.IsAppRegistered)
                        throw new InvalidOperationException("هذا الرقم مسجل بالفعل في التطبيق! يمكنك تسجيل الدخول مباشرة.");

                    var usernameTakenByOtherStudent = await _context.Students
                        .AnyAsync(s => s.Id != existingStudentByPhone.Id && s.Username.ToLower() == cleanUsername, cancellationToken);
                    if (usernameTakenByOtherStudent)
                        throw new InvalidOperationException("اسم المستخدم هذا محجوز بالفعل، يرجى اختيار اسم مستخدم آخر.");

                    var passwordHash = _passwordHasher.HashPassword(request.Password);
                    existingStudentByPhone.CompleteAppRegistration(passwordHash);
                    existingStudentByPhone.PlainPassword = request.Password;
                    existingStudentByPhone.FullName = request.FullName.Trim();
                    existingStudentByPhone.Username = cleanUsername;
                    existingStudentByPhone.DateOfBirth = request.DateOfBirth;
                    existingStudentByPhone.Gender = request.Gender;
                    existingStudentByPhone.FatherOfConfession = request.FatherOfConfession.Trim();
                    existingStudentByPhone.AcademicYear = request.AcademicYear;
                    existingStudentByPhone.College = request.College.Trim();

                    if (!string.IsNullOrWhiteSpace(request.DeviceToken))
                        existingStudentByPhone.DeviceToken = request.DeviceToken;

                    targetStudent = existingStudentByPhone;
                }
                else
                {
                    // ج) شاب جديد تماماً غير موجود مسبقاً في الداتابيز
                    var usernameTakenByStudent = await _context.Students
                        .AnyAsync(s => s.Username.ToLower() == cleanUsername, cancellationToken);
                    if (usernameTakenByStudent)
                        throw new InvalidOperationException("اسم المستخدم هذا محجوز بالفعل، يرجى اختيار اسم مستخدم آخر.");

                    var passwordHash = _passwordHasher.HashPassword(request.Password);
                    var newStudent = new Student(
                        request.FullName.Trim(),
                        cleanUsername,
                        phoneVo,
                        passwordHash,
                        request.DateOfBirth,
                        request.Gender,
                        request.FatherOfConfession.Trim(),
                        request.AcademicYear,
                        request.College.Trim()
                    );

                    if (!string.IsNullOrWhiteSpace(request.DeviceToken))
                        newStudent.DeviceToken = request.DeviceToken;

                    newStudent.PlainPassword = request.Password;
                    _context.Students.Add(newStudent);
                    targetStudent = newStudent;
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            return new AuthResponseDto
            {
                UserId = targetStudent.Id,
                FullName = targetStudent.FullName,
                Username = targetStudent.Username,
                PhoneNumber = targetStudent.PhoneNumber.Value,
                Role = UserRole.Student,
                AcademicYear = targetStudent.AcademicYear,
                QrToken = targetStudent.QrToken,
                Token = GenerateToken(targetStudent.Id, UserRole.Student.ToString(), targetStudent.FullName)
            };
        }

        public async Task<bool> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
                throw new ArgumentException("كلمة المرور الجديدة يجب ألا تقل عن 6 أحرف.");

            // البحث أولاً في الخدام
            var servant = await _context.Servants
                .FirstOrDefaultAsync(s => s.Id == request.UserId, cancellationToken);

            if (servant != null)
            {
                var isOldValid = _passwordHasher.VerifyPassword(request.CurrentPassword, servant.PasswordHash ?? string.Empty);
                if (!isOldValid)
                    throw new UnauthorizedAccessException("كلمة المرور الحالية غير صحيحة.");

                servant.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
                servant.PlainPassword = request.NewPassword;
                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }

            // البحث في الشباب
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == request.UserId, cancellationToken);

            if (student != null)
            {
                var isOldValid = _passwordHasher.VerifyPassword(request.CurrentPassword, student.PasswordHash ?? string.Empty);
                if (!isOldValid)
                    throw new UnauthorizedAccessException("كلمة المرور الحالية غير صحيحة.");

                student.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
                student.PlainPassword = request.NewPassword;
                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }

            throw new KeyNotFoundException("المستخدم غير موجود.");
        }
        public async Task<bool> AdminSetPasswordAsync(AdminSetPasswordRequest request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
                throw new ArgumentException("كلمة المرور الجديدة يجب ألا تقل عن 6 أحرف.");

            if (string.IsNullOrWhiteSpace(request.UserType))
                throw new ArgumentException("نوع المستخدم مطلوب (servant أو student).");

            var userType = request.UserType.Trim().ToLower();
            var newHash = _passwordHasher.HashPassword(request.NewPassword);

            if (userType == "servant")
            {
                var servant = await _context.Servants
                    .FirstOrDefaultAsync(s => s.Id == request.UserId, cancellationToken);

                if (servant == null)
                    throw new KeyNotFoundException("الخادم غير موجود.");

                servant.PasswordHash = newHash;
                servant.PlainPassword = request.NewPassword;
                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }
            else if (userType == "student")
            {
                var student = await _context.Students
                    .FirstOrDefaultAsync(s => s.Id == request.UserId, cancellationToken);

                if (student == null)
                    throw new KeyNotFoundException("الشاب غير موجود.");

                student.PasswordHash = newHash;
                student.PlainPassword = request.NewPassword;
                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }

            throw new ArgumentException("نوع المستخدم غير صالح. يجب أن يكون servant أو student.");
        }


        private static string CleanPhone(string phone)
        {
            return phone.Trim()
                .Replace(" ", "")
                .Replace("-", "")
                .Replace("(", "")
                .Replace(")", "");
        }

        private static string GenerateToken(int userId, string role, string fullName)
        {
            var payload = $"{userId}:{role}:{fullName}:{DateTime.UtcNow.Ticks}";
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(payload));
            return Convert.ToBase64String(hash);
        }
    }
}

