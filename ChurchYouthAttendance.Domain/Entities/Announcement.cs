using System;

namespace ChurchYouthAttendance.Domain.Entities
{
    public class Announcement
    {
        // Parameterless constructor for EF Core
        protected Announcement() { }

        public Announcement(string title, string body, int senderServantId, int? targetAcademicYear = null)
        {
            Validate(title, body, senderServantId);

            Title = title.Trim();
            Body = body.Trim();
            SenderServantId = senderServantId;
            TargetAcademicYear = targetAcademicYear;
            SentAt = DateTime.UtcNow;
        }

        private static void Validate(string title, string body, int senderServantId)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("عنوان الإعلان مطلوب ولا يمكن أن يكون فارغاً.", nameof(title));

            if (string.IsNullOrWhiteSpace(body))
                throw new ArgumentException("نص الإعلان مطلوب ولا يمكن أن يكون فارغاً.", nameof(body));

            if (senderServantId <= 0)
                throw new ArgumentException("معرف الخادم المرسل غير صالح.", nameof(senderServantId));
        }

        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Body { get; set; } = null!;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        // السنة المستهدفة: null يعني موجه لكل شباب الاجتماع، أو رقم دفعة محددة (1، 2، 3، ...)
        public int? TargetAcademicYear { get; set; }

        // الخادم أو أمين الخدمة الذي أرسل الإعلان
        public int SenderServantId { get; set; }
        public Servant Sender { get; set; } = null!;
    }
}
