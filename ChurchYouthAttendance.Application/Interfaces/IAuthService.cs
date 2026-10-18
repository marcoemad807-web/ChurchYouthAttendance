using ChurchYouthAttendance.Application.DTOs.Auth;
using ChurchYouthAttendance.Domain.Enum;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ChurchYouthAttendance.Application.Interfaces
{
    public interface IAuthService
    {
        Task<List<StudentLookupDto>> GetUnregisteredStudentsLookupAsync(
            int academicYear,
            UserGender gender,
            string? query = null,
            CancellationToken cancellationToken = default);

        Task<AuthResponseDto> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

        Task<AuthResponseDto> RegisterStudentAsync(StudentSelfRegisterRequest request, CancellationToken cancellationToken = default);

        Task<bool> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// تعيين كلمة مرور جديدة للمستخدم بواسطة أمين الخدمة (بدون الحاجة لكلمة المرور الحالية)
        /// </summary>
        Task<bool> AdminSetPasswordAsync(AdminSetPasswordRequest request, CancellationToken cancellationToken = default);
    }
}
