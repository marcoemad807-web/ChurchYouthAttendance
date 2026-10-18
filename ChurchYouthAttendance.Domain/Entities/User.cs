using ChurchYouthAttendance.Domain.Enum;
using ChurchYouthAttendance.Domain.ValueObjects;
using System;

namespace ChurchYouthAttendance.Domain.Entities
{
    public abstract class User
    {
        // Parameterless constructor for EF Core
        protected User() { }

        // Base domain constructor
        protected User(
            string fullName,
            string username,
            PhoneNumber phoneNumber,
            DateOnly dateOfBirth,
            UserGender gender,
            string fatherOfConfession,
            UserRole role,
            string? passwordHash = null)
        {
            Validate(fullName, username, phoneNumber, dateOfBirth, fatherOfConfession);

            FullName = fullName.Trim();
            Username = username.Trim().ToLower();
            PhoneNumber = phoneNumber;
            DateOfBirth = dateOfBirth;
            Gender = gender;
            FatherOfConfession = fatherOfConfession.Trim();
            Role = role;
            PasswordHash = passwordHash;
            CreatedAt = DateTime.UtcNow;
        }

        private static void Validate(string fullName, string username, PhoneNumber phoneNumber, DateOnly dateOfBirth, string fatherOfConfession)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                throw new ArgumentException("الاسم بالكامل مطلوب ولا يمكن أن يكون فارغاً.", nameof(fullName));

            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("اسم المستخدم بالإنجليزية مطلوب ولا يمكن أن يكون فارغاً.", nameof(username));

            ArgumentNullException.ThrowIfNull(phoneNumber, nameof(phoneNumber));

            if (string.IsNullOrWhiteSpace(fatherOfConfession))
                throw new ArgumentException("اسم أب الاعتراف مطلوب ولا يمكن أن يكون فارغاً.", nameof(fatherOfConfession));

            if (dateOfBirth >= DateOnly.FromDateTime(DateTime.Today))
                throw new ArgumentException("تاريخ الميلاد يجب أن يكون في الماضي.", nameof(dateOfBirth));
        }

        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Username { get; set; } = null!; // اسم المستخدم بالإنجليزية (مثل: mina.adel) - مطلوب وفريد
        public PhoneNumber PhoneNumber { get; set; } = null!;
        public string? PasswordHash { get; set; }
        public string? PlainPassword { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public UserGender Gender { get; set; }
        public string FatherOfConfession { get; set; } = null!;
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
