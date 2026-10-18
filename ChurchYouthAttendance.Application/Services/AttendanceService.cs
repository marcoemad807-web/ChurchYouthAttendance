using ChurchYouthAttendance.Application.Common.Interfaces;
using ChurchYouthAttendance.Application.DTOs.Attendance;
using ChurchYouthAttendance.Application.Interfaces;
using ChurchYouthAttendance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ChurchYouthAttendance.Application.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IAppDbContext _context;
        private readonly IAttendanceNotificationService _notificationService;

        public AttendanceService(IAppDbContext context, IAttendanceNotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<AttendanceResult> RecordAttendanceAsync(RecordAttendanceRequest request, CancellationToken cancellationToken = default)
        {
            // 1. التحقق من وجود الاجتماع وأنه مفتوح للتسجيل
            var meeting = await _context.Meetings.FirstOrDefaultAsync(m => m.Id == request.MeetingId, cancellationToken);
            if (meeting == null)
            {
                return AttendanceResult.NotFound("الاجتماع المحدد غير موجود.");
            }

            if (!meeting.IsOpen)
            {
                return AttendanceResult.MeetingClosed("الاجتماع مغلق حالياً، لا يمكن تسجيل حضور جديد.");
            }

            // 2. التحقق من وجود الشاب
            var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == request.StudentId, cancellationToken);
            if (student == null)
            {
                return AttendanceResult.NotFound("المخدوم (الشاب) غير موجود.");
            }

            // 3. التحقق من وجود الخادم
            var servant = await _context.Servants.FirstOrDefaultAsync(s => s.Id == request.ServantId, cancellationToken);
            if (servant == null)
            {
                return AttendanceResult.NotFound("الخادم المحدد غير موجود.");
            }

            // 4. فحص استباقي لمنع التكرار (Optimistic Pre-check)
            var existingAttendance = await _context.Attendances
                .Include(a => a.RecordedBy)
                .FirstOrDefaultAsync(a => a.StudentId == request.StudentId && a.MeetingId == request.MeetingId, cancellationToken);

            if (existingAttendance != null)
            {
                var recordedBy = existingAttendance.RecordedBy?.FullName ?? "خادم آخر";
                return AttendanceResult.AlreadyRecorded($"الشاب {student.FullName} مسجل حضوره بالفعل في هذا الاجتماع بواسطة الخادم: {recordedBy}.");
            }

            // 5. إنشاء سجل الحضور الجديد
            var attendance = new Attendance(
                studentId: request.StudentId,
                meetingId: request.MeetingId,
                servantId: request.ServantId,
                method: request.Method,
                notes: request.Notes
            );

            _context.Attendances.Add(attendance);

            // 6. الحفظ مع معالجة الـ Concurrency والـ Race Condition
            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                return AttendanceResult.ConcurrencyConflict("حدث تعارض في البيانات بالتزامن. يرجى إعادة المحاولة.");
            }
            catch (DbUpdateException)
            {
                // في حالة حاول خادمان التسجيل في نفس الفيمتو ثانية وعدّى الـ Pre-check للاثنين (Race Condition)
                // الـ Unique Index في قاعدة البيانات يمنع الإدخال المكرر ويرمي DbUpdateException
                var doubleCheck = await _context.Attendances
                    .Include(a => a.RecordedBy)
                    .FirstOrDefaultAsync(a => a.StudentId == request.StudentId && a.MeetingId == request.MeetingId, cancellationToken);

                if (doubleCheck != null)
                {
                    var recordedBy = doubleCheck.RecordedBy?.FullName ?? "خادم آخر";
                    return AttendanceResult.AlreadyRecorded($"تم تسجيل حضور {student.FullName} بالتزامن بواسطة الخادم: {recordedBy}.");
                }

                throw;
            }

            // 7. إرسال إشعار لحظي عبر SignalR لأمين الخدمة العام وخدام نفس الدفعة
            var notification = new AttendanceNotificationDto
            {
                AttendanceId = attendance.Id,
                StudentId = student.Id,
                StudentName = student.FullName,
                AcademicYear = student.AcademicYear,
                MeetingId = meeting.Id,
                RecordedByServantName = servant.FullName,
                AttendedAt = attendance.AttendedAt,
                Method = attendance.Method,
                Notes = attendance.Notes,
                IsAttended = true
            };

            await _notificationService.NotifyAttendanceRecordedAsync(notification, cancellationToken);

            return AttendanceResult.Success(notification, $"تم تسجيل حضور {student.FullName} بنجاح.");
        }

        public async Task<AttendanceResult> RecordAttendanceByQrAsync(ScanQrAttendanceRequest request, CancellationToken cancellationToken = default)
        {
            var rawToken = !string.IsNullOrWhiteSpace(request.QrToken)
                ? request.QrToken.Trim()
                : (request.QrCodeData?.Trim() ?? string.Empty);

            if (string.IsNullOrWhiteSpace(rawToken))
            {
                return AttendanceResult.Failure("Ø±Ù…Ø² Ø§Ù„Ù€ QR ØºÙŠØ± ØµØ§Ù„Ø­.");
            }

            // 1. Ø§Ù„Ø¨Ø­Ø« Ø¹Ù† Ø§Ù„Ø´Ø§Ø¨ Ø¨Ø±Ù…Ø² Ø§Ù„Ù€ QrToken Ø£ÙˆÙ„Ø§Ù‹
            var student = await _context.Students
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.QrToken == rawToken, cancellationToken);

            // 2. Ø¯Ø¹Ù… Ø§Ù„Ø£Ù†Ù…Ø§Ø· Ø§Ù„Ø¨Ø¯ÙŠÙ„Ø©: YOUTH_QR_{id} Ø£Ùˆ STUDENT:{id} Ø£Ùˆ Ø±Ù‚Ù… ID Ø§Ù„Ø´Ø§Ø¨ Ù…Ø¨Ø§Ø´Ø±Ø©
            if (student == null)
            {
                int studentId = 0;
                if (rawToken.StartsWith("YOUTH_QR_", StringComparison.OrdinalIgnoreCase))
                {
                    int.TryParse(rawToken.Substring("YOUTH_QR_".Length), out studentId);
                }
                else if (rawToken.StartsWith("STUDENT:", StringComparison.OrdinalIgnoreCase))
                {
                    int.TryParse(rawToken.Substring("STUDENT:".Length), out studentId);
                }
                else if (int.TryParse(rawToken, out var directId))
                {
                    studentId = directId;
                }

                if (studentId > 0)
                {
                    student = await _context.Students
                        .AsNoTracking()
                        .FirstOrDefaultAsync(s => s.Id == studentId, cancellationToken);
                }
            }

            if (student == null)
            {
                return AttendanceResult.NotFound("Ù„Ù… ÙŠØªÙ… Ø§Ù„Ø¹Ø«ÙˆØ± Ø¹Ù„Ù‰ Ù…Ø®Ø¯ÙˆÙ… Ù…Ø·Ø§Ø¨Ù‚ Ù„Ø±Ù…Ø² Ø§Ù„Ù€ QR Ø§Ù„Ù…Ù…Ø³ÙˆØ­.");
            }

            return await RecordAttendanceAsync(new RecordAttendanceRequest
            {
                StudentId = student.Id,
                MeetingId = request.MeetingId,
                ServantId = request.ServantId,
                Method = Domain.Enum.AttendanceMethod.QrCode,
                Notes = request.Notes
            }, cancellationToken);
        }

        public async Task<AttendanceResult> RemoveAttendanceAsync(int studentId, int meetingId, int servantId, CancellationToken cancellationToken = default)
        {
            var attendance = await _context.Attendances
                .Include(a => a.Student)
                .FirstOrDefaultAsync(a => a.StudentId == studentId && a.MeetingId == meetingId, cancellationToken);

            if (attendance == null)
            {
                return AttendanceResult.NotFound("سجل الحضور غير موجود أو تم إلغاؤه بالفعل.");
            }

            var student = attendance.Student;
            var servant = await _context.Servants.FirstOrDefaultAsync(s => s.Id == servantId, cancellationToken);
            var servantName = servant?.FullName ?? "أحد الخدام";

            _context.Attendances.Remove(attendance);

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                return AttendanceResult.ConcurrencyConflict("تم تعديل أو حذف هذا السجل بالتزامن من خادم آخر.");
            }

            // إشعار لحظي بإلغاء الحضور لخدام نفس الدفعة وأمين الخدمة
            await _notificationService.NotifyAttendanceRemovedAsync(
                studentId: student.Id,
                studentName: student.FullName,
                academicYear: student.AcademicYear,
                meetingId: meetingId,
                removedByServantName: servantName,
                cancellationToken: cancellationToken
            );

            return AttendanceResult.Success(new AttendanceNotificationDto
            {
                AttendanceId = attendance.Id,
                StudentId = student.Id,
                StudentName = student.FullName,
                AcademicYear = student.AcademicYear,
                MeetingId = meetingId,
                RecordedByServantName = servantName,
                AttendedAt = DateTime.UtcNow,
                IsAttended = false
            }, $"تم إلغاء حضور {student.FullName} بنجاح.");
        }

        public async Task<List<StudentAttendanceDto>> GetMeetingAttendanceByYearAsync(int meetingId, int academicYear, CancellationToken cancellationToken = default)
        {
            // جلب جميع شباب الدفعة مع حالة حضورهم في الاجتماع المحدد
            var students = await _context.Students
                .AsNoTracking()
                .Where(s => s.AcademicYear == academicYear)
                .OrderBy(s => s.FullName)
                .ToListAsync(cancellationToken);

            var attendances = await _context.Attendances
                .AsNoTracking()
                .Include(a => a.RecordedBy)
                .Where(a => a.MeetingId == meetingId && a.Student.AcademicYear == academicYear)
                .ToDictionaryAsync(a => a.StudentId, cancellationToken);

            return students.Select(s =>
            {
                var isAttended = attendances.TryGetValue(s.Id, out var att);
                return new StudentAttendanceDto
                {
                    StudentId = s.Id,
                    StudentName = s.FullName,
                    PhoneNumber = s.PhoneNumber.Value,
                    AcademicYear = s.AcademicYear,
                            Gender = (int)s.Gender,
                            College = s.College,
                    IsAttended = isAttended,
                    AttendedAt = att?.AttendedAt,
                    Method = att?.Method,
                    RecordedByServantName = att?.RecordedBy?.FullName,
                    Notes = att?.Notes
                };
            }).ToList();
        }

        public async Task<ManualAttendanceSheet> GetManualAttendanceSheetAsync(int meetingId, CancellationToken cancellationToken = default)
        {
            var meeting = await _context.Meetings.AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == meetingId, cancellationToken);

            if (meeting == null)
                throw new ArgumentException("الاجتماع المحدد غير موجود.", nameof(meetingId));

            // جلب كل الشباب مقسمين بالدفعة
            var allStudents = await _context.Students
                .AsNoTracking()
                .OrderBy(s => s.AcademicYear)
                .ThenBy(s => s.FullName)
                .ToListAsync(cancellationToken);

            // جلب كل سجلات الحضور لهذا الاجتماع
            var allAttendances = await _context.Attendances
                .AsNoTracking()
                .Include(a => a.RecordedBy)
                .Where(a => a.MeetingId == meetingId)
                .ToDictionaryAsync(a => a.StudentId, cancellationToken);

            var yearLabels = new Dictionary<int, string>
            {
                { 1, "سنة أولى" },
                { 2, "سنة تانية" },
                { 3, "سنة تالتة" },
                { 4, "سنة رابعة" },
                { 5, "سنة خامسة" },
                { 6, "خريجين" }
            };

            var groups = allStudents
                .GroupBy(s => s.AcademicYear)
                .OrderBy(g => g.Key)
                .Select(g =>
                {
                    var studentDtos = g.Select(s =>
                    {
                        var isAttended = allAttendances.TryGetValue(s.Id, out var att);
                        return new StudentAttendanceDto
                        {
                            StudentId = s.Id,
                            StudentName = s.FullName,
                            PhoneNumber = s.PhoneNumber.Value,
                            AcademicYear = s.AcademicYear,
                            Gender = (int)s.Gender,
                            College = s.College,
                            IsAttended = isAttended,
                            AttendedAt = att?.AttendedAt,
                            Method = att?.Method,
                            RecordedByServantName = att?.RecordedBy?.FullName,
                            Notes = att?.Notes
                        };
                    }).ToList();

                    return new YearAttendanceGroup
                    {
                        AcademicYear = g.Key,
                        YearLabel = yearLabels.TryGetValue(g.Key, out var label) ? label : $"سنة {g.Key}",
                        TotalStudents = studentDtos.Count,
                        AttendedCount = studentDtos.Count(s => s.IsAttended),
                        Students = studentDtos
                    };
                }).ToList();

            return new ManualAttendanceSheet
            {
                MeetingId = meeting.Id,
                MeetingDate = meeting.Date.ToString("yyyy-MM-dd"),
                IsMeetingOpen = meeting.IsOpen,
                TotalStudents = allStudents.Count,
                TotalAttended = allAttendances.Count,
                Years = groups
            };
        }

        public async Task<List<StudentAttendanceDto>> SearchStudentsAsync(int meetingId, string searchTerm, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<StudentAttendanceDto>();

            var trimmed = searchTerm.Trim();

            // بحث بالاسم أو رقم التليفون
            var students = await _context.Students
                .AsNoTracking()
                .Where(s => s.FullName.Contains(trimmed) || EF.Functions.Like((string)(object)s.PhoneNumber, $"%{trimmed}%"))
                .OrderBy(s => s.AcademicYear)
                .ThenBy(s => s.FullName)
                .ToListAsync(cancellationToken);

            // جلب حالة الحضور لنتائج البحث في هذا الاجتماع
            var studentIds = students.Select(s => s.Id).ToList();
            var attendances = await _context.Attendances
                .AsNoTracking()
                .Include(a => a.RecordedBy)
                .Where(a => a.MeetingId == meetingId && studentIds.Contains(a.StudentId))
                .ToDictionaryAsync(a => a.StudentId, cancellationToken);

            return students.Select(s =>
            {
                var isAttended = attendances.TryGetValue(s.Id, out var att);
                return new StudentAttendanceDto
                {
                    StudentId = s.Id,
                    StudentName = s.FullName,
                    PhoneNumber = s.PhoneNumber.Value,
                    AcademicYear = s.AcademicYear,
                            Gender = (int)s.Gender,
                            College = s.College,
                    IsAttended = isAttended,
                    AttendedAt = att?.AttendedAt,
                    Method = att?.Method,
                    RecordedByServantName = att?.RecordedBy?.FullName,
                    Notes = att?.Notes
                };
            }).ToList();
        }

        public async Task<AttendanceReportDto> GenerateAttendanceReportAsync(AttendanceReportRequest request, CancellationToken cancellationToken = default)
        {
            if (request.FromDate > request.ToDate)
                throw new ArgumentException("تاريخ بداية التقرير لا يمكن أن يكون بعد تاريخ النهاية.");

            // 1. التحقق من وجود الخادم وصلاحياته
            var servant = await _context.Servants.AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == request.ServantId, cancellationToken);

            if (servant == null)
                throw new KeyNotFoundException("الخادم المحدد غير موجود.");

            int? effectiveAcademicYear;
            Domain.Enum.UserGender? effectiveGender = request.Gender;
            string scopeDescription;

            if (servant.Role == Domain.Enum.UserRole.Admin)
            {
                // أمين الخدمة العام: يحدد الدفعة أو يتركها null لكل الدفعات، ويحدد النوع أو يتركه للكل
                effectiveAcademicYear = request.AcademicYear;
                var yearText = effectiveAcademicYear.HasValue ? GetYearName(effectiveAcademicYear.Value) : "جميع الفصول";
                var genderText = effectiveGender.HasValue ? (effectiveGender == Domain.Enum.UserGender.Male ? "بنين فقط" : "بنات فقط") : "بنين وبنات";
                scopeDescription = $"{yearText} - {genderText}";
            }
            else
            {
                // خادم الفصل: مقيّد بدفعته المسندة إليه فقط إجبارياً
                if (!servant.AssignedAcademicYear.HasValue)
                    throw new InvalidOperationException("هذا الخادم غير مسند إليه فصل دراسي محدد.");

                effectiveAcademicYear = servant.AssignedAcademicYear.Value;
                var yearText = GetYearName(effectiveAcademicYear.Value);
                var genderText = effectiveGender.HasValue ? (effectiveGender == Domain.Enum.UserGender.Male ? "بنين فقط" : "بنات فقط") : "بنين وبنات";
                scopeDescription = $"{yearText} (فصل الخادم المسؤول) - {genderText}";
            }

            // 2. جلب الاجتماعات التي تقع في هذه الفترة الزمنية
            var meetings = await _context.Meetings.AsNoTracking()
                .Where(m => m.Date >= request.FromDate && m.Date <= request.ToDate)
                .OrderBy(m => m.Date)
                .ToListAsync(cancellationToken);

            // 3. جلب الشباب الواقعين في نطاق التقرير
            var studentsQuery = _context.Students.AsNoTracking().AsQueryable();

            if (effectiveAcademicYear.HasValue)
            {
                studentsQuery = studentsQuery.Where(s => s.AcademicYear == effectiveAcademicYear.Value);
            }

            if (effectiveGender.HasValue)
            {
                studentsQuery = studentsQuery.Where(s => s.Gender == effectiveGender.Value);
            }

            var students = await studentsQuery
                .OrderBy(s => s.AcademicYear)
                .ThenBy(s => s.FullName)
                .ToListAsync(cancellationToken);

            var meetingIds = meetings.Select(m => m.Id).ToList();
            var studentIds = students.Select(s => s.Id).ToList();

            // 4. جلب سجلات الحضور الخاصة بهذه الاجتماعات وهؤلاء الشباب
            var attendances = await _context.Attendances.AsNoTracking()
                .Where(a => meetingIds.Contains(a.MeetingId) && studentIds.Contains(a.StudentId))
                .ToListAsync(cancellationToken);

            var attendanceLookup = attendances.ToLookup(a => (a.StudentId, a.MeetingId));

            // 5. بناء ملخص كل اجتماع
            var meetingSummaries = meetings.Select(m => new ReportMeetingSummaryDto
            {
                MeetingId = m.Id,
                Date = m.Date,
                IsOpen = m.IsOpen,
                AttendedCount = attendances.Count(a => a.MeetingId == m.Id)
            }).ToList();

            // 6. بناء سجل الحضور والغياب لكل طالب
            var studentReports = students.Select(s =>
            {
                var details = meetings.Select(m =>
                {
                    var att = attendanceLookup[(s.Id, m.Id)].FirstOrDefault();
                    return new StudentMeetingAttendanceDto
                    {
                        MeetingId = m.Id,
                        MeetingDate = m.Date,
                        Attended = att != null,
                        Method = att?.Method,
                        Notes = att?.Notes
                    };
                }).ToList();

                var attendedCount = details.Count(d => d.Attended);
                var absentCount = meetings.Count - attendedCount;
                var rate = meetings.Count > 0 ? Math.Round((double)attendedCount / meetings.Count * 100, 1) : 0;

                return new StudentReportItemDto
                {
                    StudentId = s.Id,
                    FullName = s.FullName,
                    Username = s.Username,
                    PhoneNumber = s.PhoneNumber.Value,
                    AcademicYear = s.AcademicYear,
                    Gender = s.Gender,
                    College = s.College,
                    FatherOfConfession = s.FatherOfConfession,
                    IsAppRegistered = s.IsAppRegistered,
                    AttendedMeetingsCount = attendedCount,
                    AbsentMeetingsCount = absentCount,
                    AttendancePercentage = rate,
                    MeetingDetails = details
                };
            }).ToList();

            // 7. حصر الشباب المحتاجين افتقاد ومتابعة
            var needsFollowUp = studentReports
                .Where(sr => sr.AttendancePercentage < 50.0 ||
                             (meetings.Count >= 2 && sr.MeetingDetails.TakeLast(2).All(d => !d.Attended)))
                .OrderBy(sr => sr.AttendancePercentage)
                .ThenBy(sr => sr.FullName)
                .ToList();

            // 8. إحصائيات البنين والبنات
            var maleStudents = students.Where(s => s.Gender == Domain.Enum.UserGender.Male).Select(s => s.Id).ToHashSet();
            var femaleStudents = students.Where(s => s.Gender == Domain.Enum.UserGender.Female).Select(s => s.Id).ToHashSet();

            var maleAttCount = attendances.Count(a => maleStudents.Contains(a.StudentId));
            var femaleAttCount = attendances.Count(a => femaleStudents.Contains(a.StudentId));

            var malePossible = maleStudents.Count * meetings.Count;
            var femalePossible = femaleStudents.Count * meetings.Count;

            var maleSummary = new GenderAttendanceSummary
            {
                TotalStudents = maleStudents.Count,
                TotalAttendedRecords = maleAttCount,
                AttendanceRate = malePossible > 0 ? Math.Round((double)maleAttCount / malePossible * 100, 1) : 0
            };

            var femaleSummary = new GenderAttendanceSummary
            {
                TotalStudents = femaleStudents.Count,
                TotalAttendedRecords = femaleAttCount,
                AttendanceRate = femalePossible > 0 ? Math.Round((double)femaleAttCount / femalePossible * 100, 1) : 0
            };

            // 9. إحصائيات الفصول
            var classSummaries = students
                .GroupBy(s => s.AcademicYear)
                .OrderBy(g => g.Key)
                .Select(g =>
                {
                    var classStudentIds = g.Select(s => s.Id).ToHashSet();
                    var classAttCount = attendances.Count(a => classStudentIds.Contains(a.StudentId));
                    var classPossible = classStudentIds.Count * meetings.Count;
                    return new ClassAttendanceSummary
                    {
                        AcademicYear = g.Key,
                        ClassName = GetYearName(g.Key),
                        TotalStudents = classStudentIds.Count,
                        TotalAttendedRecords = classAttCount,
                        AttendanceRate = classPossible > 0 ? Math.Round((double)classAttCount / classPossible * 100, 1) : 0
                    };
                }).ToList();

            // 10. الحسابات الإجمالية
            var totalRecords = attendances.Count;
            var totalPossible = students.Count * meetings.Count;
            var overallRate = totalPossible > 0 ? Math.Round((double)totalRecords / totalPossible * 100, 1) : 0;
            var avgPerMeeting = meetings.Count > 0 ? Math.Round((double)totalRecords / meetings.Count, 1) : 0;

            return new AttendanceReportDto
            {
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                RequestedByServantName = servant.FullName,
                ServantRole = servant.Role,
                ScopeDescription = scopeDescription,
                TotalMeetingsCount = meetings.Count,
                Meetings = meetingSummaries,
                TotalStudentsCount = students.Count,
                TotalAttendanceRecords = totalRecords,
                AverageAttendancePerMeeting = avgPerMeeting,
                OverallAttendanceRate = overallRate,
                MaleSummary = maleSummary,
                FemaleSummary = femaleSummary,
                ClassSummaries = classSummaries,
                Students = studentReports,
                NeedsFollowUpStudents = needsFollowUp
            };
        }

        private static string GetYearName(int year) => year switch
        {
            1 => "سنة أولى",
            2 => "سنة تانية",
            3 => "سنة تالتة",
            4 => "سنة رابعة",
            5 => "سنة خامسة",
            6 => "خريجين",
            _ => $"سنة {year}"
        };
    }
}
