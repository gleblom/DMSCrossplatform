using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace DMSCrossplatform.Services;

public interface IFileService
{
    Task<IStorageFile?> OpenFileAsync();
}