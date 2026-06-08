using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DMSCrossplatform.Infrastructure.Storage;

public interface IDeviceIdentityStore
{
    Task<string> GetOrCreateAsync(CancellationToken ct = default);
}

public sealed class FileDeviceIdentityStore : IDeviceIdentityStore
{
    private readonly string _path;

    public FileDeviceIdentityStore(string path) => _path = path;

    public async Task<string> GetOrCreateAsync(CancellationToken ct = default)
    {
        if (File.Exists(_path))
            return (await File.ReadAllTextAsync(_path, ct)).Trim();

        var id = Guid.NewGuid().ToString("N");
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        await File.WriteAllTextAsync(_path, id, ct);
        return id;
    }
}