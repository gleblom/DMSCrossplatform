using System.Threading.Tasks;
using Android.Content;

namespace DMSCrossplatform.Infrastructure.Android;

public interface IAndroidGetFcmToken
{ 
   Task<string> GetToken(Context context);
   
}