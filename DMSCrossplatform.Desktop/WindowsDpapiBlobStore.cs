using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DMSCrossplatform.Infrastructure.Storage;

namespace DMSCrossplatform.Desktop;



public sealed class WindowsDpapiBlobStore : ISessionBlobStore
{
    private static readonly byte[] Entropy = "DMSCrossplatform.v1"u8.ToArray();
    private readonly string _dir;

    public WindowsDpapiBlobStore()
    {
        _dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DMSCrossplatform",
            "secure");

        Directory.CreateDirectory(_dir);
    }

    public ValueTask<string?> ReadAsync(string key)
    {
        var path = GetPath(key);
        if (!File.Exists(path))
            return ValueTask.FromResult<string?>(null);

        var protectedBytes = File.ReadAllBytes(path);
        var clearBytes = ProtectedData.Unprotect(protectedBytes, Entropy, DataProtectionScope.CurrentUser);
        return ValueTask.FromResult<string?>(Encoding.UTF8.GetString(clearBytes));
    }

    public ValueTask WriteAsync(string key, string value)
    {
        var clearBytes = Encoding.UTF8.GetBytes(value);
        var protectedBytes = ProtectedData.Protect(clearBytes, Entropy, DataProtectionScope.CurrentUser);
        File.WriteAllBytes(GetPath(key), protectedBytes);
        return ValueTask.CompletedTask;
    }

    public ValueTask DeleteAsync(string key)
    {
        var path = GetPath(key);
        if (File.Exists(path))
            File.Delete(path);

        return ValueTask.CompletedTask;
    }

    private string GetPath(string key) => Path.Combine(_dir, $"{key}.bin");
}