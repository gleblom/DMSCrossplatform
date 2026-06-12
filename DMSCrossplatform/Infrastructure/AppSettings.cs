namespace DMSCrossplatform.Infrastructure;

public class AppSettings
{
    public string ApiBaseUrl { get; set; } = "https://linuxserver.tailea0f78.ts.net/";
    public int MaxLoginAttempts { get; set; } = 5;
    public int LoginLockoutSeconds { get; set; } = 60;

    public int ButtonThrottleMs { get; set; } = 600;
}