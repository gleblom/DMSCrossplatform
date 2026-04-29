using System.Threading.Tasks;

namespace DMSCrossplatform.Infrastructure.Storage;

public interface ISessionBlobStore
{
    ValueTask<string?> ReadAsync(string key);
    ValueTask WriteAsync(string key, string value);
    ValueTask DeleteAsync(string key);
}