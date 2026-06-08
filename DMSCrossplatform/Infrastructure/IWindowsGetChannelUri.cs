using System.Threading.Tasks;

namespace DMSCrossplatform.Infrastructure;

public interface IWindowsGetChannelUri
{
    Task<string?> GetPushChannel();
}