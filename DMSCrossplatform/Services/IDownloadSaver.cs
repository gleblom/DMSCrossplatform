using System;
using System.Threading;
using System.Threading.Tasks;

namespace DMSCrossplatform.Services;

public interface IDownloadSaver
{
    Task SaveAsync(Uri source, string suggestedFileName, string mimeType, CancellationToken ct = default);
}