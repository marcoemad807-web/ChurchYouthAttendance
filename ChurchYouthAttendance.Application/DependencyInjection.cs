using ChurchYouthAttendance.Application.Common.Security;
using ChurchYouthAttendance.Application.Interfaces;
using ChurchYouthAttendance.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ChurchYouthAttendance.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Security Services
            services.AddSingleton<IPasswordHasher, PasswordHasher>();

            // Application Core Services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IStudentService, StudentService>();
            services.AddScoped<IServantService, ServantService>();
            services.AddScoped<IMeetingService, MeetingService>();
            services.AddScoped<IAttendanceService, AttendanceService>();
            services.AddScoped<IAnnouncementService, AnnouncementService>();
            services.AddScoped<IFollowUpService, FollowUpService>();

            return services;
        }
    }
}
