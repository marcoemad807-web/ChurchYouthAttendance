using ChurchYouthAttendance.Application.DTOs.Student;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ChurchYouthAttendance.Application.Interfaces
{
    public interface IStudentService
    {
        /// <summary>
        /// جلب شباب دفعة محددة (فصل الخادم) مع علامة صح لمن سجل في الأبلكيشن
        /// </summary>
        Task<List<StudentDto>> GetStudentsByYearAsync(int academicYear, CancellationToken cancellationToken = default);

        /// <summary>
        /// جلب جميع الفصول مقسمة بالدفعات وإحصائيات المسجلين (خاص بأمين الخدمة)
        /// </summary>
        Task<List<ClassGroupDto>> GetAllClassesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// جلب بيانات شاب معين بالـ Id
        /// </summary>
        Task<StudentDto?> GetStudentByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// إضافة شاب جديد في كشوفات الكنيسة بواسطة الخادم أو أمين الخدمة
        /// </summary>
        Task<StudentDto> CreateStudentAsync(CreateStudentRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// تعديل بيانات شاب
        /// </summary>
        Task<StudentDto> UpdateStudentAsync(int id, UpdateStudentRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// حذف شاب
        /// </summary>
        Task<bool> DeleteStudentAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// جلب كود الـ QR الخاص بالشاب لعرضه في التطبيق
        /// </summary>
        Task<string?> GetQrTokenAsync(int studentId, CancellationToken cancellationToken = default);

        /// <summary>
        /// إعادة توليد كود الـ QR للشاب في حالة تسريبه أو الرغبة في تجديده
        /// </summary>
        Task<string> RegenerateQrTokenAsync(int studentId, CancellationToken cancellationToken = default);
    }
}
