using System;
using Newtonsoft.Json;

namespace DMSCrossplatform.Models.Dto;

public class DeviceRegisterDto
{
    
    [JsonProperty("platform")]
    public string? Platform { get; set; }
    
    [JsonProperty("push_token")]
    public string? PushToken { get; set; }
    
    [JsonProperty("device_id")]
    public string? DeviceId { get; set; }
}
public class ReadDeviceDto
{
    [JsonProperty("id")] 
    public Guid Id { get; set; }
    
    [JsonProperty("user_id")] 
    public Guid UserId { get; set; }
    
    [JsonProperty("platform")]
    public string? Platform { get; set; }
    
    [JsonProperty("push_token")]
    public string? PushToken { get; set; }
    
    [JsonProperty("device_id")]
    public string? DeviceId { get; set; }
    
    [JsonProperty("is_active")]
    public bool IsActive { get; set; }
    
    [JsonProperty("push_enabled")]
    public bool PushEnabled { get; set; }
    
    [JsonProperty("last_seen_at")]
    public DateTime? LastSeenAt { get; set; }
    
    [JsonProperty("last_push_at")]
    public DateTime? LastPushAt { get; set; }
    [JsonProperty("disabled_at")]
    public DateTime? DisabledAt { get; set; }
    
    [JsonProperty("last_push_status")]
    public string? LastPushStatus { get; set; }
    
    [JsonProperty("last_push_error")]
    public string? LastPushError { get; set; }
    
    
}

public class PushNotificationData
{

    [JsonProperty("notification_id")]
    public int NotificationId { get; set; }
    
    [JsonProperty("document_id")]
    public Guid DocumentId { get; set; }
    
    [JsonProperty("event_type")]
    public string? EventType { get; set; }
    
    [JsonProperty("step_index")]
    public int StepIndex { get; set; }
    
    [JsonProperty("reason")]
    public string? Reason { get; set; }
    
}


public class PushSettingsReadDto
{
    [JsonProperty("user_id")]
    public Guid UserId { get; set; }
    
    [JsonProperty("push_enabled")]
    public bool PushEnabled { get; set; }
}

public class PushSettingsUpdateDto
{
    [JsonProperty("push_enabled")]
    public bool PushEnabled { get; set; }
}

public class DeviceUpdateDto
{
    [JsonProperty("push_enabled")]
    public bool PushEnabled { get; set; }
}

public class DeviceStatusDto
{
    [JsonProperty("device_push_enabled")]
    public bool DevicePushEnabled { get; set; }
    [JsonProperty("user_push_enabled")]
    public bool UserPushEnabled { get; set; }
    [JsonProperty("platform")]
    public string? Platform { get; set; }
    [JsonProperty("device_id")]
    public string? DeviceId { get; set; }
    [JsonProperty("last_seen_at")]
    public DateTime? LastSeenAt { get; set; }
    
    [JsonProperty("last_push_at")]
    public DateTime? LastPushAt { get; set; }
    [JsonProperty("disabled_at")]
    public DateTime? DisabledAt { get; set; }
    
    [JsonProperty("last_push_status")]
    public string? LastPushStatus { get; set; }
    
    [JsonProperty("last_push_error")]
    public string? LastPushError { get; set; }
}
public class DeviceListItemDto: DeviceStatusDto
{
    [JsonProperty("id")]
    public Guid Id { get; set; }
}

