using System;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;


namespace DMSCrossplatform.Models.Dto
{
    public class UserCreateDto
    {
        [JsonProperty("email")] public string Email { get; set; } = string.Empty;
        [JsonProperty("password")] public string Password { get; set; } = string.Empty;
        [JsonProperty("phone")] public string? Phone { get; set; }
    }

    public class UserReadDto
    {
        [JsonProperty("id")] public Guid Id { get; set; }
        [JsonProperty("email")] public string Email { get; set; } = string.Empty;
        [JsonProperty("phone")] public string? Phone { get; set; }
        [JsonProperty("is_active")] public bool IsActive { get; set; }
        [JsonProperty("is_email_verified")] public bool IsEmailVerified { get; set; }
        [JsonProperty("created_at")] public DateTime CreatedAt { get; set; }
    }

    public class UserPublicDto
    {
        [JsonProperty("id")] public Guid Id { get; set; }
    }

    public class UserTokenDto
    {
        [JsonProperty("access_token")] public string AccessToken { get; set; } = string.Empty;
        [JsonProperty("token_type")] public string TokenType { get; set; } = "bearer";
        [JsonProperty("refresh_token")] public string? RefreshToken { get; set; } = string.Empty;
    }

    public class UserFullDto
    {
        [JsonProperty("user_id")] public Guid UserId { get; set; }
        [JsonProperty("user_email")] public string? Email { get; set; }
        [JsonProperty("user_phone")] public string? Phone { get; set; }
        [JsonProperty("is_active")] public bool? IsActive { get; set; }
        [JsonProperty("is_email_verified")] public bool? IsEmailVerified { get; set; }
        [JsonProperty("first_name")] public string? FirstName { get; set; }
        [JsonProperty("second_name")] public string? SecondName { get; set; }
        [JsonProperty("third_name")] public string? ThirdName { get; set; }
        [JsonProperty("company_id")] public Guid? CompanyId { get; set; }
        [JsonProperty("company_name")] public string? CompanyName { get; set; }
        [JsonProperty("otp_enabled")] public bool OtpEnabled { get; set; }
        [JsonProperty("otp_verified")] public bool OtpVerified { get; set; }
        [JsonProperty("role_id")] public int? RoleId { get; set; }
        [JsonProperty("role_name")] public string? RoleName { get; set; }
        [JsonProperty("unit_id")] public int? UnitId { get; set; }
        [JsonProperty("unit_name")] public string? UnitName { get; set; }
        [JsonProperty("user_created_at")] public DateTime CreatedAt { get; set; }
        [JsonProperty("passkey_enabled")] public bool? PasskeyEnabled { get; set; }
        
        [NotMapped]
        public string? FullName => $"{FirstName} {SecondName} {ThirdName}";
    }

    public class ProfileDto
    {
        [JsonProperty("id")] public Guid Id { get; set; }
        [JsonProperty("first_name")] public string? FirstName { get; set; }
        [JsonProperty("second_name")] public string? SecondName { get; set; }
        [JsonProperty("third_name")] public string? ThirdName { get; set; }
        [JsonProperty("role_id")] public int? RoleId { get; set; }
        [JsonProperty("unit_id")] public int? UnitId { get; set; }
        [JsonProperty("company_id")] public Guid? CompanyId { get; set; }
    }

    public class UserUpdateDto
    {
        [JsonProperty("phone")] public string? Phone { get; set; }
        [JsonProperty("is_active")] public bool? IsActive { get; set; }
        [JsonProperty("director_id")] public Guid? DirectorId { get; set; }
        [JsonProperty("company_id")] public Guid? CompanyId { get; set; }
    }
}