using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content.PM;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using DMSCrossplatform.Infrastructure.Android;

namespace DMSCrossplatform.Android;

public class AndroidActivityHost: IAndroidActivityHost
{
    private static Activity? _current;

    public Activity? Current => _current;

    public static void SetCurrent(Activity activity)
    {
        _current = activity;
    }
}

public class AndroidPermissionRequester: IAndroidPermissionRequester
{
    private const int CameraRequestCode = 1001;
    private const int NotificationRequestCode = 1234;
    private static TaskCompletionSource<bool>? _pending;
    
    

    public Task<bool> RequestCameraAsync(Activity activity)
    {
        if (ContextCompat.CheckSelfPermission(activity, Manifest.Permission.Camera) == Permission.Granted)
            return Task.FromResult(true);

        _pending = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        ActivityCompat.RequestPermissions(activity, new[] { Manifest.Permission.Camera }, CameraRequestCode);
        return _pending.Task;
    }

    public Task<bool> RequestNotificationAsync(Activity activity)
    {
        if (ContextCompat.CheckSelfPermission(activity, Manifest.Permission.PostNotifications) == Permission.Granted)
            return Task.FromResult(true);

        _pending = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        ActivityCompat.RequestPermissions(activity, new[] { Manifest.Permission.PostNotifications } , NotificationRequestCode);
        return _pending.Task;
    }

    public static void HandleRequestPermissionsResult(int requestCode, Permission[] grantResults)
    {
        if (requestCode != CameraRequestCode || _pending is null)
            return;

        var granted = grantResults.Length > 0 && grantResults[0] == Permission.Granted;
        _pending.TrySetResult(granted);
        _pending = null;
    }


}