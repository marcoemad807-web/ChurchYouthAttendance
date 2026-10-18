using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace ChurchYouthAttendance.API.Hubs
{
    public class AttendanceHub : Hub
    {
        /// <summary>
        /// انضمام خادم إلى مجموعة الدفعة الخاصة به لاستقبال تحديثات الحضور فورياً
        /// </summary>
        public async Task JoinYearGroup(int academicYear)
        {
            var groupName = GetYearGroupName(academicYear);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        /// <summary>
        /// مغادرة مجموعة الدفعة
        /// </summary>
        public async Task LeaveYearGroup(int academicYear)
        {
            var groupName = GetYearGroupName(academicYear);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        /// <summary>
        /// انضمام أمين الخدمة العام (Admin) للاستماع لكل الفصول والدفعات لحظياً
        /// </summary>
        public async Task JoinAdminsGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
        }

        /// <summary>
        /// مغادرة مجموعة أمناء الخدمة
        /// </summary>
        public async Task LeaveAdminsGroup()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Admins");
        }

        /// <summary>
        /// انضمام الشاب إلى مجموعة الشباب العامة لاستقبال الإعلانات الموجهة للكل
        /// </summary>
        public async Task JoinStudentsGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Students");
        }

        /// <summary>
        /// مغادرة مجموعة الشباب
        /// </summary>
        public async Task LeaveStudentsGroup()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Students");
        }

        public static string GetYearGroupName(int academicYear) => $"Year_{academicYear}";

        public override Task OnConnectedAsync()
        {
            // عند اتصال خادم أو أمين خدمة
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            return base.OnDisconnectedAsync(exception);
        }
    }
}
