using Android.App;

namespace DMSCrossplatform.Infrastructure.Android;

public interface IAndroidActivityHost
{
    Activity? Current { get; }
}