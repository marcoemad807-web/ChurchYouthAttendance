using ChurchYouthAttendance.Application.DTOs.Servant;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ChurchYouthAttendance.Application.Interfaces
{
    public interface IServantService
    {
        /// <summary>
        /// جلب جميع الخدام مع الدفعات المسندة إليهم (خاص بأمين الخدمة)
        /// </summary>
        Task<List<ServantDto>> GetAllServantsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// جلب بيانات خادم بالـ Id
        /// </summary>
        Task<ServantDto?> GetServantByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// إضافة خادم جديد وتحديد دفعته المسندة وكلمة مروره (خاص بأمين الخدمة)
        /// </summary>
        Task<ServantDto> CreateServantAsync(CreateServantRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// تعديل بيانات خادم أو تغيير دفعته المسندة
        /// </summary>
        Task<ServantDto> UpdateServantAsync(int id, UpdateServantRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// حذف خادم
        /// </summary>
        Task<bool> DeleteServantAsync(int id, CancellationToken cancellationToken = default);
    }
}
