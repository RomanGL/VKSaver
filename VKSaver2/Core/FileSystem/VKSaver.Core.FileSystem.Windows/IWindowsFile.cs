using Windows.Storage;

namespace VKSaver.Core.FileSystem
{
    public interface IWindowsFile : IFile
    {
        StorageFile StorageFile { get; }
    }
}
