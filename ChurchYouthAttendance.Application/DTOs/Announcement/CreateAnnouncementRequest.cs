namespace ChurchYouthAttendance.Application.DTOs.Announcement
{
    public class CreateAnnouncementRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public int SenderServantId { get; set; }

        /// <summary>
        /// null = إعلان عام لكل شباب الاجتماع، أو رقم الدفعة (مثلاً 1، 2، 3، 4)
        /// </summary>
        public int? TargetAcademicYear { get; set; }
    }
}
