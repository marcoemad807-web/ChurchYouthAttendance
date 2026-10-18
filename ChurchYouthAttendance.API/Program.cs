using ChurchYouthAttendance.API.Hubs;
using ChurchYouthAttendance.API.Services;
using ChurchYouthAttendance.Application;
using ChurchYouthAttendance.Application.Common.Interfaces;
using ChurchYouthAttendance.Application.Interfaces;
using ChurchYouthAttendance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// إعداد AppDbContext مع SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
});

// تسجيل IAppDbContext للـ Application layer
builder.Services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

// تسجيل خدمات طبقة الـ Application (Clean Architecture)
builder.Services.AddApplication();

// تسجيل خدمات البنية التحتية (Infrastructure Services) الخاصة بالـ API
builder.Services.AddScoped<IAttendanceNotificationService, SignalRAttendanceNotificationService>();
builder.Services.AddScoped<IPushNotificationService, FirebasePushNotificationService>();

// إضافة SignalR للإشعارات اللحظية
builder.Services.AddSignalR();

// إعداد CORS لتطبيق Flutter والموبايل
builder.Services.AddCors(options =>
{
    options.AddPolicy("FlutterAppPolicy", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// إعداد Swagger و OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();

var app = builder.Build();

// تشغيل Seeding البيانات الأولية عند بدء التطبيق
await AppDbSeeder.SeedAsync(app.Services);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ChurchYouthAttendance API v1");
    });
    app.MapOpenApi();
}

// app.UseHttpsRedirection(); // Disabled for local development / Android Emulator HTTP

app.UseCors("FlutterAppPolicy");

app.UseAuthorization();

app.MapControllers();

// Endpoint الخاص بـ SignalR Hub
app.MapHub<AttendanceHub>("/hubs/attendance");

app.Run();
