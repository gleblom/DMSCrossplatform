using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Provider;
using DMSCrossplatform.Services;
using Environment = Android.OS.Environment;

namespace DMSCrossplatform.Android;

public sealed class AndroidDownloadSaver : IDownloadSaver
{
    private static Context Context  => global::Android.App.Application.Context;
    

    public async Task SaveAsync(Uri source, string suggestedFileName, string mimeType, CancellationToken ct = default)
    {
        var resolver = Context.ContentResolver;
        var collection = MediaStore.Downloads.GetContentUri(MediaStore.VolumeExternalPrimary);

        var values = new ContentValues();
        values.Put(MediaStore.IMediaColumns.DisplayName, suggestedFileName);
        values.Put(MediaStore.IMediaColumns.MimeType, mimeType);
        values.Put(MediaStore.IMediaColumns.RelativePath, Environment.DirectoryDownloads + "/Document Service");
        values.Put(MediaStore.IMediaColumns.IsPending, 1);

        var itemUri = resolver.Insert(collection, values)
                      ?? throw new IOException("Failed to create MediaStore item.");

        try
        {
            using var http = new HttpClient();
            await using var input = await http.GetStreamAsync(source, ct);
            await using var output = resolver.OpenOutputStream(itemUri)
                                     ?? throw new IOException("Failed to open output stream.");

            await input.CopyToAsync(output, ct);
        }
        finally
        {
            values.Clear();
            values.Put(MediaStore.IMediaColumns.IsPending, 0);
            resolver.Update(itemUri, values, null, null);
        }
    }
}