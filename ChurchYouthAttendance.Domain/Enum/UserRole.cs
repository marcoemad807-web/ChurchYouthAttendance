namespace ChurchYouthAttendance.Domain.Enum
{
    public enum UserRole
    {
        Student = 1,        // الشاب (طالب)
        Servant = 2,        // خادم / خادمة الفصل (مرتبط بدفعة معينة وبنفس نوعه)
        Admin = 3           // أمين الخدمة (المشرف العام - يرى كل الدفعات والولاد والبنات)
    }
}
