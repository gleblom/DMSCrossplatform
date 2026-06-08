using System;
using System.Threading.Tasks;
using DMSCrossplatform.Infrastructure;
using Microsoft.Windows.PushNotifications;

namespace DMSCrossplatform.Desktop;

public class WindowsPushNotifications: IWindowsGetChannelUri
{
    private static readonly Guid PushNotificationRemoteId = new("6b45745a-eb12-467e-8929-13b060b287c6");
    

    public async Task<string?> GetPushChannel()
    {
        try
        {
            if(!PushNotificationManager.IsSupported())
                return null;
            
            var pushManager = PushNotificationManager.Default;
            var result = await pushManager.CreateChannelAsync(PushNotificationRemoteId);
            var channelUri = result.Channel.Uri.ToString();
            
            return channelUri;
        }
        catch
        {
            return null;
        }
    }
}
