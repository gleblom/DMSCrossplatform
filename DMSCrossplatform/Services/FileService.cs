using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace DMSCrossplatform.Services;

public class FileService: IFileService
{
    private readonly TopLevel _target;
    public FileService(TopLevel target)
    {
        _target = target;
    }
    
    public async Task<IStorageFile?> OpenFileAsync()
    {
        var files = await _target.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open File",
            AllowMultiple = false
        });
        return files.FirstOrDefault();
    }
}