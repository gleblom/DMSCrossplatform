using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DMSCrossplatform.Services;

public interface IDownloadSaver
{
    Task SaveAsync(Stream content, string suggestedFileName, string mimeType, CancellationToken ct = default);
}
public static class DownloadSaverExtensions
{
    public static Task SaveAsync(this IDownloadSaver saver, byte[] content, string suggestedFileName, string mimeType, CancellationToken ct = default)
        => saver.SaveAsync(new MemoryStream(content, writable: false), suggestedFileName, mimeType, ct);
}