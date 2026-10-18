using ChurchYouthAttendance.Application.Common.Interfaces;
using ChurchYouthAttendance.Application.DTOs.FollowUp;
using ChurchYouthAttendance.Application.Interfaces;
using ChurchYouthAttendance.Domain.Enum;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ChurchYouthAttendance.Application.Services
{
    public class FollowUpService : IFollowUpService
    {
        private readonly IAppDbContext _context;

        public FollowUpService(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<List<MissingStudentDto>> GetMissingStudentsAsync(
            int? academicYear = null,
            UserGender? gender = null,
            int minConsecutiveAbsences = 2,
            CancellationToken cancellationToken = default)
        {
            if (minConsecutiveAbsences < 1)
                minConsecutiveAbsences = 1;

            // 1. جلب الاجتماعات السابقة مرتبة تنازلياً حسب التاريخ
            var recentMeetings = await _context.Meetings
                .AsNoTracking()
                .OrderByDescending(m => m.Date)
                .Take(20) // فحص آخر 20 اجتماع كحد أقصى لحساب الغياب المتتالي
                .ToListAsync(cancellationToken);

            if (recentMeetings.Count == 0)
                return new List<MissingStudentDto>();

            // 2. جلب قائمة الشباب وفق الفلاتر المحددة
            var studentsQuery = _context.Students
                .AsNoTracking()
                .AsQueryable();

            if (academicYear.HasValue)
            {
                studentsQuery = studentsQuery.Where(s => s.AcademicYear == academicYear.Value);
            }

            if (gender.HasValue)
            {
                studentsQuery = studentsQuery.Where(s => s.Gender == gender.Value);
            }

            var students = await studentsQuery
                .OrderBy(s => s.AcademicYear)
                .ThenBy(s => s.FullName)
                .ToListAsync(cancellationToken);

            if (students.Count == 0)
                return new List<MissingStudentDto>();

            var studentIds = students.Select(s => s.Id).ToList();

            // 3. جلب جميع سجلات الحضور لهؤلاء الشباب في الاجتماعات المحددة
            var meetingIds = recentMeetings.Select(m => m.Id).ToList();
            var attendances = await _context.Attendances
                .AsNoTracking()
                .Where(a => studentIds.Contains(a.StudentId) && meetingIds.Contains(a.MeetingId))
                .Select(a => new { a.StudentId, a.MeetingId })
                .ToListAsync(cancellationToken);

            var studentMeetingLookup = attendances
                .ToLookup(a => a.StudentId, a => a.MeetingId);

            // 4. جلب تاريخ آخر حضور عام لكل طالب (حتى لو قبل آخر 20 اجتماع)
            var lastAttendedLookup = await _context.Attendances
                .AsNoTracking()
                .Where(a => studentIds.Contains(a.StudentId))
                .GroupBy(a => a.StudentId)
                .Select(g => new
                {
                    StudentId = g.Key,
                    LastDate = g.Max(a => a.Meeting.Date)
                })
                .ToDictionaryAsync(x => x.StudentId, x => (DateOnly?)x.LastDate, cancellationToken);

            var missingStudents = new List<MissingStudentDto>();

            foreach (var student in students)
            {
                var attendedMeetingIds = studentMeetingLookup[student.Id].ToHashSet();

                // حساب عدد أسابيع الغياب المتتالية بدءاً من أحدث اجتماع
                int consecutiveAbsences = 0;
                foreach (var meeting in recentMeetings)
                {
                    if (attendedMeetingIds.Contains(meeting.Id))
                    {
                        // حضر هذا الاجتماع -> توقف العد
                        break;
                    }
                    consecutiveAbsences++;
                }

                if (consecutiveAbsences >= minConsecutiveAbsences)
                {
                    lastAttendedLookup.TryGetValue(student.Id, out var lastAttendedDate);

                    missingStudents.Add(new MissingStudentDto
                    {
                        StudentId = student.Id,
                        FullName = student.FullName,
                        PhoneNumber = student.PhoneNumber.Value,
                        AcademicYear = student.AcademicYear,
                        College = student.College,
                        FatherOfConfession = student.FatherOfConfession,
                        ConsecutiveAbsenceWeeks = consecutiveAbsences,
                        LastAttendedDate = lastAttendedDate
                    });
                }
            }

            return missingStudents
                .OrderByDescending(s => s.ConsecutiveAbsenceWeeks)
                .ThenBy(s => s.AcademicYear)
                .ThenBy(s => s.FullName)
                .ToList();
        }

        public async Task<List<MissingStudentDto>> GetMissingStudentsForServantAsync(
            int servantId,
            int minConsecutiveAbsences = 2,
            CancellationToken cancellationToken = default)
        {
            var servant = await _context.Servants
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == servantId, cancellationToken);

            if (servant == null)
                throw new KeyNotFoundException("الخادم المحدد غير موجود.");

            if (servant.Role == UserRole.Admin)
            {
                // أمين الخدمة العام يرى جميع الشباب المحتاجين افتقاد
                return await GetMissingStudentsAsync(
                    academicYear: null,
                    gender: null,
                    minConsecutiveAbsences: minConsecutiveAbsences,
                    cancellationToken: cancellationToken);
            }

            // خادم الفصل يرى شباب فصله ونفس نوعه
            return await GetMissingStudentsAsync(
                academicYear: servant.AssignedAcademicYear,
                gender: servant.Gender,
                minConsecutiveAbsences: minConsecutiveAbsences,
                cancellationToken: cancellationToken);
        }
    }
}
