using System.Collections.Generic;
using System.Threading.Tasks;
using DMSCrossplatform.Models.Dto;

namespace DMSCrossplatform.Services;

public interface IPushService
{
    Task<ReadDeviceDto> RegisterDevice(DeviceRegisterDto deviceRegisterDto);

    Task DeleteNotification(int notificationId);
    
    Task MarkRead();
    
    Task<MvNotificationsDto> GetNotification(int notificationId);
    
    Task<IReadOnlyList<MvNotificationsDto>> GetNotifications();
    
    Task<PushSettingsReadDto> GetPushSettings();
    
    Task<PushSettingsReadDto> UpdatePushSettings(PushSettingsUpdateDto pushSettingsUpdateDto);
    
    Task<IReadOnlyList<DeviceListItemDto>> GetDevices();
    
    Task<DeviceStatusDto> GetDeviceStatus(string deviceId);
    
    Task<ReadDeviceDto> UpdateDeviceStatus(string deviceId, DeviceUpdateDto deviceUpdateDto);
}