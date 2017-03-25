using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace VKSaver.Core.FileSystem
{
    public interface IFolder
    {
        string DisplayName { get; }
        string Path { get; }

        Task<IFile> GetFileAsync(string name);
        Task<IFolder> GetFolderAsync(string name);

        Task<IReadOnlyList<IFile>> GetFilesAsync();
        Task<IReadOnlyList<IFolder>> GetFoldersAsync();
        Task<IReadOnlyList<IFile>> GetFilesAsync(uint startIndex, uint maxItemsToRetrieve);

        Task<IFile> CreateFileAsync(string desiredName);
        Task<IFile> CreateFileAsync(string desiredName, CreationCollisionOption options);

        Task DeleteAsync(bool isPermanent);
    }
}
