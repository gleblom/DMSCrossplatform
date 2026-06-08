using System.Collections.Generic;
using System.Threading.Tasks;
using DMSCrossplatform.Infrastructure.Api;
using DMSCrossplatform.Models.Dto;

namespace DMSCrossplatform.Services;

public class PushService: IPushService
{
    private readonly IApiClient _api;
    
    public PushService(IApiClient api) => _api = api;

    public Task<ReadDeviceDto> RegisterDevice(DeviceRegisterDto deviceRegisterDto)
        => _api.PostJsonAsync<DeviceRegisterDto, ReadDeviceDto>("api/push/devices/register", deviceRegisterDto);

    public Task DeleteNotification(int notificationId)
        => _api.DeleteAsync($"api/push/delete/{notificationId}");
    
    public Task MarkRead()
        => _api.PostJsonAsync<object>("api/push/mark/read",new {});
    
    public Task<MvNotificationsDto>  GetNotification(int notificationId)
        => _api.GetAsync<MvNotificationsDto>($"api/push/{notificationId}");

    public Task<IReadOnlyList<MvNotificationsDto>> GetNotifications()
        => _api.GetAsync<IReadOnlyList<MvNotificationsDto>>("api/push/notifications/all");

    public Task<PushSettingsReadDto> GetPushSettings()
        => _api.GetAsync<PushSettingsReadDto>("api/push/settings");

    public Task<PushSettingsReadDto> UpdatePushSettings(PushSettingsUpdateDto pushSettingsUpdateDto)
        => _api.PostJsonAsync<PushSettingsUpdateDto, PushSettingsReadDto>("api/push/settings", pushSettingsUpdateDto);

    public Task<IReadOnlyList<DeviceListItemDto>> GetDevices()
        => _api.GetAsync<IReadOnlyList<DeviceListItemDto>>("api/push/devices");

    public Task<DeviceStatusDto> GetDeviceStatus(string deviceId)
        => _api.GetAsync<DeviceStatusDto>($"api/push/device/{deviceId}");

    public Task<ReadDeviceDto> UpdateDeviceStatus(string deviceId, DeviceUpdateDto deviceUpdateDto)
        => _api.PostJsonAsync<DeviceUpdateDto, ReadDeviceDto>($"api/push/device/{deviceId}", deviceUpdateDto);
}