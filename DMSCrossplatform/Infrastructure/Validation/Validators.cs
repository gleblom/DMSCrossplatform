using System.Text.RegularExpressions;

namespace DMSCrossplatform.Infrastructure.Validation;

public static class Validators
{
    private static readonly Regex EmojiRegex = new(
        @"[\uD800-\uDBFF][\uDC00-\uDFFF]|[\u2600-\u27BF\uE000-\uF8FF\uFE0F]",
        RegexOptions.Compiled);

    private static readonly Regex NameRegex = new(
        @"^[A-Za-zА-Яа-яЁё]{1,32}$", RegexOptions.Compiled);

    private static readonly Regex EntityNameRegex = new(
        @"^[A-Za-zА-Яа-яЁё0-9 _\-()«»:;/\\""']{1,64}$", RegexOptions.Compiled);

    private static readonly Regex EmailRegex = new(
        @"^[^@\s]{1,40}@[^@\s]{1,40}\.[^@\s]{2,10}$", RegexOptions.Compiled);

    private static readonly Regex PhoneRegex = new(
        @"^(\+7|8)?\d{10,11}$", RegexOptions.Compiled);

    public static bool ContainsEmoji(string? input)
        => !string.IsNullOrEmpty(input) && EmojiRegex.IsMatch(input);

    public static string? ValidateName(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value)) return $"{fieldName}: поле обязательно для заполнения.";
        if (ContainsEmoji(value)) return $"{fieldName}: недопустимые символы.";
        if (value.Length > 32) return $"{fieldName}: не более 32 символов.";
        if (!NameRegex.IsMatch(value)) return $"{fieldName}: разрешены только русские и латинские буквы.";
        return null;
    }

    public static string? ValidateEmail(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "Email обязателен.";
        if (ContainsEmoji(value)) return "Email содержит недопустимые символы.";
        if (value.Length is < 5 or > 50) return "Email должен быть от 5 до 50 символов.";
        if (!EmailRegex.IsMatch(value)) return "Некорректный формат email.";
        return null;
    }

    public static string? ValidatePassword(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "Пароль обязателен.";
        if (ContainsEmoji(value)) return "Пароль содержит недопустимые символы.";
        if (value.Length < 8) return "Пароль должен содержать не менее 8 символов.";
        return null;
    }

    public static string? ValidatePhone(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null; // телефон не всегда обязателен
        var clean = value.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
        if (!PhoneRegex.IsMatch(clean)) return "Некорректный формат телефона.";
        return null;
    }

    public static string? ValidateEntityName(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value)) return $"{fieldName}: поле обязательно.";
        if (ContainsEmoji(value)) return $"{fieldName}: недопустимые символы.";
        if (!EntityNameRegex.IsMatch(value)) return $"{fieldName}: разрешены буквы, цифры и символы _-()«»:;/\\.";
        return null;
    }
}