using System;
using System.IO;
using System.Threading.Tasks;

namespace VKSaver.Core.FileSystem
{
    public interface IFile : IDisposable
    {
        string Name { get; }
        string DisplayName { get; }
        string FileType { get; }
        string Path { get; }
        IFileProperties Properties { get; }

        Task<Stream> OpenAsync(FileAccessMode accessMode);
        Task DeleteAsync(bool isPermanent = true);
        Task RenameAsync(string desiredName, NameCollisionOption option);
    }
}
