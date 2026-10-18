using ChurchYouthAttendance.Domain.Entities;
using ChurchYouthAttendance.Domain.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ChurchYouthAttendance.Infrastructure.Data
{
    public static class AppDbSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // تأكد من تطبيق كل الـ Migrations أولاً
            try
            {
                await context.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Seeder] MigrateAsync note: {ex.Message}");
            }

            // =========================================================
            // أ) إضافة الخدام والشباب والاجتماعات الأولية (إذا لم تكن موجودة)
            // =========================================================
            if (!await context.Servants.AnyAsync())
            {
                // =============================================
                // 1. أمين الخدمة العام (Admin)
                // =============================================
                var admin = new Servant(
                    fullName: "أبانوب ناجي",
                    username: "abanoub.nagi",
                    phoneNumber: "01000000001",
                    passwordHash: "AQAAAAIAAYagAAAAEHashed_Admin_Password_Placeholder",
                    dateOfBirth: new DateOnly(1985, 3, 15),
                    gender: UserGender.Male,
                    fatherOfConfession: "أبونا تادرس",
                    role: UserRole.Admin
                );

                // =============================================
                // 2. خدام الفصول (Servants) - كل خادم مسؤول عن دفعة
                // =============================================
                var servantYear1 = new Servant(
                    fullName: "مينا سمير عبدالملاك",
                    username: "mina.samir",
                    phoneNumber: "01100000001",
                    passwordHash: "AQAAAAIAAYagAAAAEHashed_Servant1_Password",
                    dateOfBirth: new DateOnly(1995, 7, 20),
                    gender: UserGender.Male,
                    fatherOfConfession: "أبونا يوسف",
                    role: UserRole.Servant,
                    assignedAcademicYear: 1
                );

                var servantYear2 = new Servant(
                    fullName: "كيرلس عادل فوزي",
                    username: "kyrollos.adel",
                    phoneNumber: "01100000002",
                    passwordHash: "AQAAAAIAAYagAAAAEHashed_Servant2_Password",
                    dateOfBirth: new DateOnly(1996, 11, 5),
                    gender: UserGender.Male,
                    fatherOfConfession: "أبونا أنطونيوس",
                    role: UserRole.Servant,
                    assignedAcademicYear: 2
                );

                var servantYear3 = new Servant(
                    fullName: "جورج ماهر حنا",
                    username: "george.maher",
                    phoneNumber: "01100000003",
                    passwordHash: "AQAAAAIAAYagAAAAEHashed_Servant3_Password",
                    dateOfBirth: new DateOnly(1994, 4, 12),
                    gender: UserGender.Male,
                    fatherOfConfession: "أبونا بيشوي",
                    role: UserRole.Servant,
                    assignedAcademicYear: 3
                );

                var servantYear4 = new Servant(
                    fullName: "مريم نبيل عزيز",
                    username: "mary.nabil",
                    phoneNumber: "01100000004",
                    passwordHash: "AQAAAAIAAYagAAAAEHashed_Servant4_Password",
                    dateOfBirth: new DateOnly(1997, 9, 30),
                    gender: UserGender.Female,
                    fatherOfConfession: "أبونا داود",
                    role: UserRole.Servant,
                    assignedAcademicYear: 4
                );

                context.Servants.AddRange(admin, servantYear1, servantYear2, servantYear3, servantYear4);
                await context.SaveChangesAsync();

                // =============================================
                // 3. الشباب (Students) - موزعين على 4 دفعات
                // =============================================

                // --- دفعة أولى (AcademicYear = 1) ---
                var studentsYear1 = new[]
                {
                    new Student("أندرو سامي ميلاد",    "andrew.sami",     "01200000001", new DateOnly(2005, 1, 10), UserGender.Male,   "أبونا يوسف",     1, "هندسة"),
                    new Student("بولس ماجد فهمي",       "boulos.maged",    "01200000002", new DateOnly(2005, 5, 22), UserGender.Male,   "أبونا يوسف",     1, "طب"),
                    new Student("مارينا عماد وهبة",     "marina.emad",     "01200000003", new DateOnly(2005, 8, 3),  UserGender.Female, "أبونا يوسف",     1, "صيدلة"),
                    new Student("يوسف عاطف سعد",        "youssef.atef",    "01200000004", new DateOnly(2005, 12, 17),UserGender.Male,   "أبونا يوسف",     1, "حاسبات ومعلومات"),
                    new Student("فيرونيكا ثروت ذكي",   "veronica.tharwat", "01200000005", new DateOnly(2005, 3, 9),  UserGender.Female, "أبونا يوسف",     1, "تجارة"),
                };

                // --- دفعة تانية (AcademicYear = 2) ---
                var studentsYear2 = new[]
                {
                    new Student("مايكل عادل جرجس",      "michael.adel",    "01200000006", new DateOnly(2004, 2, 14), UserGender.Male,   "أبونا أنطونيوس", 2, "هندسة"),
                    new Student("كريستين رأفت حبيب",    "christine.raafat", "01200000007", new DateOnly(2004, 6, 28), UserGender.Female, "أبونا أنطونيوس", 2, "علوم"),
                    new Student("بيتر كمال منصور",       "peter.kamal",     "01200000008", new DateOnly(2004, 10, 7), UserGender.Male,   "أبونا أنطونيوس", 2, "حقوق"),
                    new Student("ساندرا نادر يوسف",     "sandra.nader",    "01200000009", new DateOnly(2004, 4, 19), UserGender.Female, "أبونا أنطونيوس", 2, "آداب"),
                    new Student("مارك وجيه فرج",        "mark.wagih",      "01200000010", new DateOnly(2004, 9, 1),  UserGender.Male,   "أبونا أنطونيوس", 2, "تربية"),
                };

                // --- دفعة تالتة (AcademicYear = 3) ---
                var studentsYear3 = new[]
                {
                    new Student("دانيال ناجي سعيد",     "daniel.nagi",     "01200000011", new DateOnly(2003, 3, 5),  UserGender.Male,   "أبونا بيشوي",    3, "طب أسنان"),
                    new Student("إيريني مجدي لبيب",     "irene.magdy",     "01200000012", new DateOnly(2003, 7, 18), UserGender.Female, "أبونا بيشوي",    3, "هندسة"),
                    new Student("جوزيف شريف فايز",      "joseph.sherif",   "01200000013", new DateOnly(2003, 11, 25),UserGender.Male,   "أبونا بيشوي",    3, "حاسبات ومعلومات"),
                    new Student("مريم أشرف ميخائيل",    "mary.ashraf",     "01200000014", new DateOnly(2003, 1, 30), UserGender.Female, "أبونا بيشوي",    3, "فنون جميلة"),
                };

                // --- دفعة رابعة (AcademicYear = 4) ---
                var studentsYear4 = new[]
                {
                    new Student("فيلوباتير وائل جورج",  "philopateer.wael", "01200000015", new DateOnly(2002, 4, 12), UserGender.Male,   "أبونا داود",     4, "طب"),
                    new Student("مونيكا سامح بشرى",     "monica.sameh",    "01200000016", new DateOnly(2002, 8, 20), UserGender.Female, "أبونا داود",     4, "صيدلة"),
                    new Student("صموئيل رمزي عزت",      "samuel.ramzy",    "01200000017", new DateOnly(2002, 6, 7),  UserGender.Male,   "أبونا داود",     4, "هندسة"),
                    new Student("جوي هاني نسيم",        "joy.hany",        "01200000018", new DateOnly(2002, 2, 28), UserGender.Female, "أبونا داود",     4, "ألسن"),
                };

                context.Students.AddRange(studentsYear1);
                context.Students.AddRange(studentsYear2);
                context.Students.AddRange(studentsYear3);
                context.Students.AddRange(studentsYear4);
                await context.SaveChangesAsync();

                // تفعيل بعض الشباب كأنهم سجلوا على الأبلكيشن وعندهم QR Code و Device Token
                studentsYear1[0].CompleteAppRegistration("AQAAAAIAAYagAAAAEHashed_Student1");
                studentsYear1[0].DeviceToken = "fcm_mock_token_student_1";

                studentsYear1[1].CompleteAppRegistration("AQAAAAIAAYagAAAAEHashed_Student2");
                studentsYear1[1].DeviceToken = "fcm_mock_token_student_2";

                studentsYear2[0].CompleteAppRegistration("AQAAAAIAAYagAAAAEHashed_Student6");
                studentsYear2[0].DeviceToken = "fcm_mock_token_student_6";

                studentsYear2[2].CompleteAppRegistration("AQAAAAIAAYagAAAAEHashed_Student8");
                studentsYear3[0].CompleteAppRegistration("AQAAAAIAAYagAAAAEHashed_Student11");
                studentsYear4[0].CompleteAppRegistration("AQAAAAIAAYagAAAAEHashed_Student15");
                await context.SaveChangesAsync();

                // =============================================
                // 4. اجتماعات (Meetings)
                // =============================================
                var today = DateOnly.FromDateTime(DateTime.Today);
                var meeting1 = new Meeting(today.AddDays(-14)); // اجتماع قبل أسبوعين (مغلق)
                meeting1.Close();
                var meeting2 = new Meeting(today.AddDays(-7));  // اجتماع الأسبوع اللي فات (مغلق)
                meeting2.Close();
                var meeting3 = new Meeting(today);              // اجتماع النهاردة (مفتوح للتسجيل)

                context.Meetings.AddRange(meeting1, meeting2, meeting3);
                await context.SaveChangesAsync();

                // =============================================
                // 5. سجلات حضور (Attendances)
                // =============================================
                var attendancesM1 = new[]
                {
                    new Attendance(studentsYear1[0].Id, meeting1.Id, servantYear1.Id, AttendanceMethod.QrCode),
                    new Attendance(studentsYear1[1].Id, meeting1.Id, servantYear1.Id, AttendanceMethod.QrCode),
                    new Attendance(studentsYear1[2].Id, meeting1.Id, servantYear1.Id, AttendanceMethod.Manual, "جات متأخرة"),
                    new Attendance(studentsYear2[0].Id, meeting1.Id, servantYear2.Id, AttendanceMethod.QrCode),
                    new Attendance(studentsYear2[1].Id, meeting1.Id, servantYear2.Id, AttendanceMethod.Manual),
                    new Attendance(studentsYear3[0].Id, meeting1.Id, servantYear3.Id, AttendanceMethod.QrCode),
                    new Attendance(studentsYear3[1].Id, meeting1.Id, servantYear3.Id, AttendanceMethod.Manual),
                    new Attendance(studentsYear4[0].Id, meeting1.Id, servantYear4.Id, AttendanceMethod.QrCode),
                };

                var attendancesM2 = new[]
                {
                    new Attendance(studentsYear1[0].Id, meeting2.Id, servantYear1.Id, AttendanceMethod.QrCode),
                    new Attendance(studentsYear1[3].Id, meeting2.Id, servantYear1.Id, AttendanceMethod.Manual),
                    new Attendance(studentsYear2[0].Id, meeting2.Id, servantYear2.Id, AttendanceMethod.QrCode),
                    new Attendance(studentsYear2[2].Id, meeting2.Id, servantYear2.Id, AttendanceMethod.QrCode),
                    new Attendance(studentsYear2[3].Id, meeting2.Id, servantYear2.Id, AttendanceMethod.Manual),
                    new Attendance(studentsYear3[2].Id, meeting2.Id, servantYear3.Id, AttendanceMethod.Manual),
                    new Attendance(studentsYear4[0].Id, meeting2.Id, servantYear4.Id, AttendanceMethod.QrCode),
                    new Attendance(studentsYear4[1].Id, meeting2.Id, servantYear4.Id, AttendanceMethod.Manual),
                    new Attendance(studentsYear4[2].Id, meeting2.Id, servantYear4.Id, AttendanceMethod.Manual),
                };

                context.Attendances.AddRange(attendancesM1);
                context.Attendances.AddRange(attendancesM2);
                await context.SaveChangesAsync();
            }

            // =========================================================
            // ب) الإعلانات (Announcements) والتنبيهات الموجهة للشباب
            // =========================================================
            if (!await context.Announcements.AnyAsync())
            {
                var admin = await context.Servants.FirstOrDefaultAsync(s => s.Role == UserRole.Admin);
                var servantY1 = await context.Servants.FirstOrDefaultAsync(s => s.AssignedAcademicYear == 1);
                var servantY4 = await context.Servants.FirstOrDefaultAsync(s => s.AssignedAcademicYear == 4);

                if (admin != null)
                {
                    var announcements = new[]
                    {
                        // إعلان عام 1: ميعاد الاجتماع
                        new Announcement(
                            title: "ميعاد اجتماع الشباب الأسبوعي القادم",
                            body: "سلام ونعمة يا شباب! مستنينكم يوم الجمعة القادم الساعة 6:30 مساءً بالكاتدرائية. هيكون معانا كلمة روحية خاصة عن (تحديات الشباب في العصر الحديث) وفقرة ترانيم مميزة. الحضور بمسح الـ QR Code من الأبلكيشن!",
                            senderServantId: admin.Id,
                            targetAcademicYear: null // إعلان عام موجه لكل الدفعات
                        ),
                        // إعلان عام 2: حجز المؤتمر السنوي
                        new Announcement(
                            title: "فتح باب الحجز لمؤتمر الشباب السنوي 2026",
                            body: "بنعمة ربنا تم فتح باب الحجز لمؤتمر الشباب السنوي في دير الأنبا بيشوي بوادي النطرون. التفاصيل والتسجيل متاحين مع خدام الفصول بعد الاجتماع مباشرة. الأعداد محدودة بأسبقية الحجز!",
                            senderServantId: admin.Id,
                            targetAcademicYear: null // إعلان عام موجه لكل الدفعات
                        )
                    };

                    context.Announcements.AddRange(announcements);
                }

                if (servantY1 != null)
                {
                    // إعلان خاص بدفعة أولى فقط
                    context.Announcements.Add(new Announcement(
                        title: "اجتماع خاص لشباب وبنات دفعة أولى جامعة",
                        body: "إخوتي شباب أولى، الجمعة دي هيكون عندنا نصف ساعة إضافية بعد الاجتماع لمناقشة تفاصيل رحلة الدفعة واستبيان الخدمة، متتأخروش!",
                        senderServantId: servantY1.Id,
                        targetAcademicYear: 1 // موجه لدفعة أولى فقط
                    ));
                }

                if (servantY4 != null)
                {
                    // إعلان خاص بدفعة رابعة فقط
                    context.Announcements.Add(new Announcement(
                        title: "ورشة عمل كتابة السيرة الذاتية (CV) للخريجين",
                        body: "يا خريجينا الأعزاء بدفعة رابعة، الجمعة القادمة الساعة 5:00 عصراً قبل الاجتماع هيكون في ورشة عمل مجانية لتأهيل الشباب لسوق العمل وكتابة الـ CV باحترافية.",
                        senderServantId: servantY4.Id,
                        targetAcademicYear: 4 // موجه لدفعة رابعة فقط
                    ));
                }

                await context.SaveChangesAsync();
            }

            // =========================================================
            // ج) تحديث رموز الأجهزة (DeviceToken) للشباب المسجلين لتجربة الإشعارات
            // =========================================================
            var registeredStudentsWithoutToken = await context.Students
                .Where(s => s.IsAppRegistered && s.DeviceToken == null)
                .ToListAsync();

            if (registeredStudentsWithoutToken.Count > 0)
            {
                foreach (var st in registeredStudentsWithoutToken)
                {
                    st.DeviceToken = $"fcm_mock_token_student_{st.Id}";
                }
                await context.SaveChangesAsync();
            }
        }
    }
}
