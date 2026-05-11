using Newtonsoft.Json;

namespace DMSCrossplatform.Models.Dto;

public class OtpDto
{
    [JsonProperty("otp_base32")]  public string? OtpBase32 { get; set; }
    [JsonProperty("otp_auth_url")]  public string? OtpAuthUrl { get; set; }
}
public class OtpVerifyDto
{
    [JsonProperty("token")]  public string? Token { get; set; }
    [JsonProperty("otp_base32")] public string? OtpBase32 { get; set; }
}