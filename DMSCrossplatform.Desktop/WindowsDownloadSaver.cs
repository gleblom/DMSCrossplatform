using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using DMSCrossplatform.Services;
using Microsoft.Extensions.Logging;

namespace DMSCrossplatform.Desktop;

public class WindowsDownloadSaver: IDownloadSaver
{
    private readonly ILogger<WindowsDownloadSaver> _log;
    private readonly IStorageProvider _storageProvider = App.storageProvider;

    public WindowsDownloadSaver(ILogger<WindowsDownloadSaver> log)
    {
        _log = log;
    }

    public async Task SaveAsync(Stream content, string suggestedFileName, string mimeType, CancellationToken ct = default)
    {
        var file = await _storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Сохранить загруженный файл",
            SuggestedFileName = suggestedFileName
        });

        if (file is null)
            return;

        await using var target = await file.OpenWriteAsync();
        await content.CopyToAsync(target, ct);
        
        _log.LogInformation("File saved to {Path}", file.Path);
    }
}