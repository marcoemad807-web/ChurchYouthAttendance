using ChurchYouthAttendance.Domain.Common;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace ChurchYouthAttendance.Domain.ValueObjects
{
    /// <summary>
    /// Value Object يمثل رقم الموبايل مع التحقق من صحته وثبات قيمته
    /// يدعم الأرقام المصرية (010, 011, 012, 015) أو الأرقام الدولية
    /// </summary>
    [JsonConverter(typeof(PhoneNumberJsonConverter))]
    public sealed record PhoneNumber : ValueObject
    {
        // نمط للتحقق: يدعم أرقام الموبايل المصرية (11 رقم تبدأ بـ 010, 011, 012, 015 أو كود مصر +201) أو صيغة دولية E.164
        private static readonly Regex PhoneRegex = new(
            @"^(?:(?:\+?20|0)?1[0125]\d{8}|\+?[1-9]\d{6,14})$",
            RegexOptions.Compiled);

        public string Value { get; }

        // Parameterless constructor لـ EF Core وعمليات الـ Serialization
        private PhoneNumber()
        {
            Value = null!;
        }

        public PhoneNumber(string value)
        {
            Value = ValidateAndClean(value);
        }

        private static string ValidateAndClean(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("رقم الموبايل لا يمكن أن يكون فارغاً.", nameof(value));

            var cleaned = CleanNumber(value);

            if (!PhoneRegex.IsMatch(cleaned))
                throw new ArgumentException($"صيغة رقم الموبايل غير صحيحة: '{value}'. يجب أن يكون رقماً مصرياً صحيحاً (مثل: 01012345678) أو دولياً معتمد.", nameof(value));

            return cleaned;
        }

        public static PhoneNumber Create(string value) => new(value);

        public static bool TryCreate(string? value, out PhoneNumber? phoneNumber, out string? errorMessage)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                phoneNumber = null;
                errorMessage = "رقم الموبايل لا يمكن أن يكون فارغاً.";
                return false;
            }

            var cleaned = CleanNumber(value);

            if (!PhoneRegex.IsMatch(cleaned))
            {
                phoneNumber = null;
                errorMessage = $"صيغة رقم الموبايل غير صحيحة: '{value}'.";
                return false;
            }

            phoneNumber = new PhoneNumber(cleaned);
            errorMessage = null;
            return true;
        }

        private static string CleanNumber(string value)
        {
            return value.Trim()
                        .Replace(" ", "")
                        .Replace("-", "")
                        .Replace("(", "")
                        .Replace(")", "");
        }

        public override string ToString() => Value;

        // التحويل الضمني (Implicit Operators) لتسهيل التعامل بين string و PhoneNumber
        public static implicit operator string?(PhoneNumber? phoneNumber) => phoneNumber?.Value;
        public static implicit operator PhoneNumber(string value) => new(value);
    }

    /// <summary>
    /// Custom JsonConverter لكي يظهر رقم الموبايل كـ string مباشر في JSON و Swagger والـ API
    /// بدلاً من كائن متداخل { "value": "010..." }
    /// </summary>
    public class PhoneNumberJsonConverter : JsonConverter<PhoneNumber>
    {
        public override PhoneNumber? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            var stringValue = reader.GetString();
            return stringValue is null ? null : new PhoneNumber(stringValue);
        }

        public override void Write(Utf8JsonWriter writer, PhoneNumber value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.Value);
        }
    }
}
