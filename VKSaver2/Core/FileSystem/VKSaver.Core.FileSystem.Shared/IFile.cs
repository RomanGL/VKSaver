using System.IO;
using System.Threading.Tasks;

namespace VKSaver.Core.FileSystem
{
    public interface IFile
    {
        string Name { get; }
        string DisplayName { get; }
        string FileType { get; }
        string Path { get; }

        Task<Stream> OpenAsync(FileAccessMode accessMode);
        Task DeleteAsync(bool isPermanent);
    }
}
