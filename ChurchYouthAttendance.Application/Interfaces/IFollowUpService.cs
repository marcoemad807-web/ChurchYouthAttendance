using ChurchYouthAttendance.Application.DTOs.FollowUp;
using ChurchYouthAttendance.Domain.Enum;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ChurchYouthAttendance.Application.Interfaces
{
    public interface IFollowUpService
    {
        /// <summary>
        /// جلب كشف الافتقاد للشباب المتغيبين لعدد معين من الاجتماعات المتتالية
        /// مع إمكانية الفلترة بالسنة الدراسية أو النوع
        /// </summary>
        Task<List<MissingStudentDto>> GetMissingStudentsAsync(
            int? academicYear = null,
            UserGender? gender = null,
            int minConsecutiveAbsences = 2,
            IEnumerable<int>? academicYears = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// جلب كشف الافتقاد الخاص بخادم معين بناءً على صلاحياته ودفعته ونوعه
        /// </summary>
        Task<List<MissingStudentDto>> GetMissingStudentsForServantAsync(
            int servantId,
            int minConsecutiveAbsences = 2,
            CancellationToken cancellationToken = default);
    }
}
