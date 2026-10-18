using ChurchYouthAttendance.Application.Common.Interfaces;
using ChurchYouthAttendance.Application.DTOs.Announcement;
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
    public class AnnouncementService : IAnnouncementService
    {
        private readonly IAppDbContext _context;
        private readonly IPushNotificationService _pushNotificationService;
        private readonly IAttendanceNotificationService _notificationService;

        public AnnouncementService(
            IAppDbContext context,
            IPushNotificationService pushNotificationService,
            IAttendanceNotificationService notificationService)
        {
            _context = context;
            _pushNotificationService = pushNotificationService;
            _notificationService = notificationService;
        }

        public async Task<AnnouncementDto> CreateAnnouncementAsync(CreateAnnouncementRequest request, CancellationToken cancellationToken = default)
        {
            var servant = await _context.Servants.FirstOrDefaultAsync(s => s.Id == request.SenderServantId, cancellationToken);
            if (servant == null)
            {
                throw new ArgumentException("الخادم المرسل غير موجود في النظام.", nameof(request.SenderServantId));
            }

            var announcement = new Announcement(
                title: request.Title,
                body: request.Body,
                senderServantId: request.SenderServantId,
                targetAcademicYear: request.TargetAcademicYear
            );

            _context.Announcements.Add(announcement);
            await _context.SaveChangesAsync(cancellationToken);

            var announcementDto = new AnnouncementDto
            {
                Id = announcement.Id,
                Title = announcement.Title,
                Body = announcement.Body,
                SentAt = announcement.SentAt,
                TargetAcademicYear = announcement.TargetAcademicYear,
                SenderServantName = servant.FullName
            };

            // 1. جلب رموز الأجهزة (Device Tokens) للشباب المستهدفين لإرسال Push Notification (FCM)
            var tokensQuery = _context.Students
                .AsNoTracking()
                .Where(s => !string.IsNullOrEmpty(s.DeviceToken));

            if (request.TargetAcademicYear.HasValue)
            {
                tokensQuery = tokensQuery.Where(s => s.AcademicYear == request.TargetAcademicYear.Value);
            }

            var deviceTokens = await tokensQuery
                .Select(s => s.DeviceToken!)
                .ToListAsync(cancellationToken);

            if (deviceTokens.Count > 0)
            {
                await _pushNotificationService.SendPushNotificationAsync(
                    deviceTokens: deviceTokens,
                    title: announcement.Title,
                    body: announcement.Body,
                    customData: new { announcementId = announcement.Id, targetYear = announcement.TargetAcademicYear },
                    cancellationToken: cancellationToken
                );
            }

            // 2. إرسال إشعار لحظي عبر SignalR (In-App) للشباب المتصلين حالياً
            await _notificationService.NotifyNewAnnouncementAsync(announcementDto, cancellationToken);

            return announcementDto;
        }

        public async Task<List<AnnouncementDto>> GetAnnouncementsForStudentAsync(int academicYear, int limit = 20, CancellationToken cancellationToken = default)
        {
            // يجلب الإعلانات العامة (لكل الدفعات) + الإعلانات المخصصة لدفعة هذا الشاب
            return await _context.Announcements
                .AsNoTracking()
                .Include(a => a.Sender)
                .Where(a => a.TargetAcademicYear == null || a.TargetAcademicYear == academicYear)
                .OrderByDescending(a => a.SentAt)
                .Take(limit)
                .Select(a => new AnnouncementDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Body = a.Body,
                    SentAt = a.SentAt,
                    TargetAcademicYear = a.TargetAcademicYear,
                    SenderServantName = a.Sender.FullName
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<List<AnnouncementDto>> GetAllAnnouncementsAsync(int limit = 50, CancellationToken cancellationToken = default)
        {
            return await _context.Announcements
                .AsNoTracking()
                .Include(a => a.Sender)
                .OrderByDescending(a => a.SentAt)
                .Take(limit)
                .Select(a => new AnnouncementDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Body = a.Body,
                    SentAt = a.SentAt,
                    TargetAcademicYear = a.TargetAcademicYear,
                    SenderServantName = a.Sender.FullName
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> UpdateStudentDeviceTokenAsync(UpdateDeviceTokenRequest request, CancellationToken cancellationToken = default)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == request.StudentId, cancellationToken);
            if (student == null)
            {
                return false;
            }

            student.DeviceToken = request.DeviceToken;
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
