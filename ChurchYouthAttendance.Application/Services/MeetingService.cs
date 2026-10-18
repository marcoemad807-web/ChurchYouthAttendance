using ChurchYouthAttendance.Application.Common.Interfaces;
using ChurchYouthAttendance.Application.DTOs.Meeting;
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
    public class MeetingService : IMeetingService
    {
        private readonly IAppDbContext _context;

        public MeetingService(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<List<MeetingDto>> GetAllMeetingsAsync(CancellationToken cancellationToken = default)
        {
            var meetings = await _context.Meetings
                .AsNoTracking()
                .Include(m => m.Attendances)
                .OrderByDescending(m => m.Date)
                .ToListAsync(cancellationToken);

            return meetings.Select(m => new MeetingDto
            {
                Id = m.Id,
                Date = m.Date,
                IsOpen = m.IsOpen,
                TotalAttended = m.Attendances.Count,
                CreatedAt = m.CreatedAt
            }).ToList();
        }

        public async Task<MeetingDto?> GetCurrentOpenMeetingAsync(CancellationToken cancellationToken = default)
        {
            var meeting = await _context.Meetings
                .AsNoTracking()
                .Include(m => m.Attendances)
                .OrderByDescending(m => m.Date)
                .FirstOrDefaultAsync(m => m.IsOpen, cancellationToken);

            if (meeting == null)
                return null;

            return new MeetingDto
            {
                Id = meeting.Id,
                Date = meeting.Date,
                IsOpen = meeting.IsOpen,
                TotalAttended = meeting.Attendances.Count,
                CreatedAt = meeting.CreatedAt
            };
        }

        public async Task<MeetingDto> CreateMeetingAsync(CreateMeetingRequest request, CancellationToken cancellationToken = default)
        {
            // التحقق من عدم وجود اجتماع آخر بنفس التاريخ
            var exists = await _context.Meetings
                .AnyAsync(m => m.Date == request.Date, cancellationToken);

            if (exists)
                throw new InvalidOperationException($"يوجد اجتماع مسجل بالفعل بتاريخ {request.Date:yyyy-MM-dd}.");

            var meeting = new Meeting(request.Date)
            {
                IsOpen = request.IsOpen
            };

            _context.Meetings.Add(meeting);
            await _context.SaveChangesAsync(cancellationToken);

            return new MeetingDto
            {
                Id = meeting.Id,
                Date = meeting.Date,
                IsOpen = meeting.IsOpen,
                TotalAttended = 0,
                CreatedAt = meeting.CreatedAt
            };
        }

        public async Task<MeetingDto> UpdateMeetingAsync(int id, UpdateMeetingRequest request, CancellationToken cancellationToken = default)
        {
            var meeting = await _context.Meetings
                .Include(m => m.Attendances)
                .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

            if (meeting == null)
                throw new KeyNotFoundException("الاجتماع المحدد غير موجود.");

            // التحقق من عدم وجود اجتماع آخر بنفس التاريخ الجديد
            var dateConflict = await _context.Meetings
                .AnyAsync(m => m.Id != id && m.Date == request.Date, cancellationToken);

            if (dateConflict)
                throw new InvalidOperationException($"يوجد اجتماع آخر مسجل بالفعل بتاريخ {request.Date:yyyy-MM-dd}.");

            meeting.Date = request.Date;
            meeting.IsOpen = request.IsOpen;

            await _context.SaveChangesAsync(cancellationToken);

            return new MeetingDto
            {
                Id = meeting.Id,
                Date = meeting.Date,
                IsOpen = meeting.IsOpen,
                TotalAttended = meeting.Attendances.Count,
                CreatedAt = meeting.CreatedAt
            };
        }

        public async Task<bool> DeleteMeetingAsync(int id, CancellationToken cancellationToken = default)
        {
            var meeting = await _context.Meetings
                .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

            if (meeting == null)
                return false;

            _context.Meetings.Remove(meeting);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<MeetingDto> ToggleMeetingStatusAsync(int id, CancellationToken cancellationToken = default)
        {
            var meeting = await _context.Meetings
                .Include(m => m.Attendances)
                .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

            if (meeting == null)
                throw new KeyNotFoundException("الاجتماع المحدد غير موجود.");

            if (meeting.IsOpen)
                meeting.Close();
            else
                meeting.Open();

            await _context.SaveChangesAsync(cancellationToken);

            return new MeetingDto
            {
                Id = meeting.Id,
                Date = meeting.Date,
                IsOpen = meeting.IsOpen,
                TotalAttended = meeting.Attendances.Count,
                CreatedAt = meeting.CreatedAt
            };
        }

        public async Task<List<MeetingDto>> GetFridayMeetingsAsync(CancellationToken cancellationToken = default)
        {
            var meetings = await _context.Meetings
                .AsNoTracking()
                .Include(m => m.Attendances)
                .OrderByDescending(m => m.Date)
                .ToListAsync(cancellationToken);

            return meetings
                .Where(m => m.Date.DayOfWeek == DayOfWeek.Friday)
                .Select(m => new MeetingDto
                {
                    Id = m.Id,
                    Date = m.Date,
                    IsOpen = m.IsOpen,
                    TotalAttended = m.Attendances.Count,
                    CreatedAt = m.CreatedAt
                }).ToList();
        }

        public async Task<List<FridayOptionDto>> GetFridayOptionsAsync(int count = 12, int futureCount = 4, CancellationToken cancellationToken = default)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            int daysUntilFriday = ((int)DayOfWeek.Friday - (int)today.DayOfWeek + 7) % 7;
            var referenceFriday = today.AddDays(daysUntilFriday);

            var fridayDates = new List<DateOnly>();
            // ??????? ??????
            for (int i = 1; i <= futureCount; i++)
            {
                fridayDates.Add(referenceFriday.AddDays(7 * i));
            }
            // ?????? ??????? + ???????
            for (int i = 0; i < count; i++)
            {
                fridayDates.Add(referenceFriday.AddDays(-7 * i));
            }
            // ????? ?????? (?????? ?????)
            fridayDates = fridayDates.OrderByDescending(d => d).ToList();

            var minDate = fridayDates.Min();
            var maxDate = fridayDates.Max();

            var existingMeetings = await _context.Meetings
                .AsNoTracking()
                .Include(m => m.Attendances)
                .Where(m => m.Date >= minDate && m.Date <= maxDate)
                .ToListAsync(cancellationToken);

            var meetingsDict = existingMeetings.ToDictionary(m => m.Date);
            var arabicCulture = new System.Globalization.CultureInfo("ar-EG");

            var result = new List<FridayOptionDto>();
            foreach (var friday in fridayDates)
            {
                meetingsDict.TryGetValue(friday, out var meeting);

                var dt = friday.ToDateTime(TimeOnly.MinValue);
                var formatted = $"الجمعة، {friday.Day} {dt.ToString("MMMM yyyy", arabicCulture)}";

                result.Add(new FridayOptionDto
                {
                    Date = friday,
                    FormattedDate = formatted,
                    IsCurrentWeek = (friday == referenceFriday),
                    MeetingId = meeting?.Id,
                    IsOpen = meeting?.IsOpen ?? false,
                    TotalAttended = meeting?.Attendances.Count ?? 0
                });
            }

            return result;
        }

        public async Task<MeetingDto> GetOrCreateMeetingByDateAsync(DateOnly date, CancellationToken cancellationToken = default)
        {
            var meeting = await _context.Meetings
                .Include(m => m.Attendances)
                .FirstOrDefaultAsync(m => m.Date == date, cancellationToken);

            if (meeting == null)
            {
                meeting = new Meeting(date)
                {
                    IsOpen = false
                };

                _context.Meetings.Add(meeting);
                await _context.SaveChangesAsync(cancellationToken);
            }

            return new MeetingDto
            {
                Id = meeting.Id,
                Date = meeting.Date,
                IsOpen = meeting.IsOpen,
                TotalAttended = meeting.Attendances.Count,
                CreatedAt = meeting.CreatedAt
            };
        }
    }
}
