

using System.Threading.Tasks;
using Android.App;

namespace DMSCrossplatform.Infrastructure.Android;

public interface IAndroidPermissionRequester
{
    Task<bool> RequestCameraAsync(Activity activity);
    
    Task<bool> RequestNotificationAsync(Activity activity);

    Task RevokeNotificationAsync(Activity activity);
}
