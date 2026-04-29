namespace DMSCrossplatform.Infrastructure;

public class AppSettings
{
    public string ApiBaseUrl { get; set; } = "http://127.0.0.1:8000";
    public int MaxLoginAttempts { get; set; } = 5;
    public int LoginLockoutSeconds { get; set; } = 60;

    public int ButtonThrottleMs { get; set; } = 600;
}