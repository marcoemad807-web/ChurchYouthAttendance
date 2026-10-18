using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ChurchYouthAttendance.Application.Interfaces
{
    public interface IPushNotificationService
    {
        /// <summary>
        /// إرسال Push Notification لأجهزة الشباب (FCM)
        /// </summary>
        Task<int> SendPushNotificationAsync(
            IEnumerable<string> deviceTokens,
            string title,
            string body,
            object? customData = null,
            CancellationToken cancellationToken = default);
    }
}
