using ChurchYouthAttendance.Application.DTOs.Meeting;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ChurchYouthAttendance.Application.Interfaces
{
    public interface IMeetingService
    {
        /// <summary>
        /// جلب كل الاجتماعات مع عدد الحضور في كل اجتماع
        /// </summary>
        Task<List<MeetingDto>> GetAllMeetingsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// جلب الاجتماع الحالي المفتوح لتسجيل الحضور
        /// </summary>
        Task<MeetingDto?> GetCurrentOpenMeetingAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// إنشاء اجتماع جديد (بواسطة أمين الخدمة أو الخادم)
        /// </summary>
        Task<MeetingDto> CreateMeetingAsync(CreateMeetingRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// تعديل بيانات اجتماع (التاريخ أو حالة الفتح/القفل)
        /// </summary>
        Task<MeetingDto> UpdateMeetingAsync(int id, UpdateMeetingRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// حذف اجتماع وسجلات الحضور التابعة له
        /// </summary>
        Task<bool> DeleteMeetingAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// فتح أو قفل تسجيل الغياب للاجتماع
        /// </summary>
        Task<MeetingDto> ToggleMeetingStatusAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// جلب اجتماعات أيام الجمعة فقط
        /// </summary>
        Task<List<MeetingDto>> GetFridayMeetingsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// جلب قائمة خيارات أيام الجمعة (السابقة والحالية) لفلتر واجهة الخدام مع حالة الاجتماع في كل جمعة
        /// </summary>
        Task<List<FridayOptionDto>> GetFridayOptionsAsync(int count = 12, int futureCount = 4, CancellationToken cancellationToken = default);

        /// <summary>
        /// جلب اجتماع ليوم جمعة محدد أو إنشائه تلقائياً إذا لم يكن موجوداً
        /// </summary>
        Task<MeetingDto> GetOrCreateMeetingByDateAsync(System.DateOnly date, CancellationToken cancellationToken = default);
    }
}
