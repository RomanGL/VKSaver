using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Collections.ObjectModel;
using Java.IO;
using File = Java.IO.File;

namespace VKSaver.Core.FileSystem
{
    public sealed class AndroidFolder : IFolder, IDisposable
    {
        private File _currentFolder;

        public AndroidFolder(string path)
        {
            _currentFolder = new File(path);

            if (!_currentFolder.IsDirectory)
                throw new ArgumentException("This is not folder.");
        }

        internal AndroidFolder(File folder)
        {
            if (folder == null)
                throw new ArgumentNullException(nameof(folder));
            if (!folder.IsDirectory)
                throw new ArgumentException("This is not folder.");

            _currentFolder = folder;
        }

        public string DisplayName => _currentFolder.Name;

        public string Path => _currentFolder.Path;

        public Task<IFile> CreateFileAsync(string desiredName)
        {
            return CreateFileAsync(desiredName, CreationCollisionOption.GenerateUniqueName);
        }

        public Task<IFile> CreateFileAsync(string desiredName, CreationCollisionOption options)
        {
            return Task.Run(() =>
            {
                IFile result = null;
                var file = new File(_currentFolder, desiredName);

                switch (options)
                {
                    case CreationCollisionOption.GenerateUniqueName:
                        int i = 1;
                        string name = System.IO.Path.GetFileNameWithoutExtension(desiredName);
                        string ext = System.IO.Path.GetExtension(desiredName);

                        while (file.Exists())
                            file = new File(_currentFolder, name + i + ext);
                        break;
                    case CreationCollisionOption.ReplaceExisting:
                        file.CreateNewFile();
                        break;
                    case CreationCollisionOption.FailIfExists:
                        if (file.Exists())
                            throw new Exception("File already exists.");
                        break;
                    case CreationCollisionOption.OpenIfExists:
                        if (!file.Exists())
                            file.CreateNewFile();
                        break;
                }

                result = new AndroidFile(file);
                return result;
            });
        }

        public Task DeleteAsync(bool isPermanent = true)
        {
            return Task.Run(() =>
            {
                var files = _currentFolder.ListFiles();
                foreach (var file in files)
                    file.Delete();

                bool result = _currentFolder.Delete();
                if (!result)
                    throw new Exception("Folder can't be deleted.");
            });
        }

        public Task<IFile> GetFileAsync(string name)
        {
            return Task.Run(() =>
            {
                var file = new File(_currentFolder, name);
                if (file.Exists())
                {
                    IFile result = new AndroidFile(file);
                    return result;
                }

                throw new System.IO.FileNotFoundException(name);
            });
        }

        public Task<IReadOnlyList<IFile>> GetFilesAsync()
        {
            return Task.Run(() =>
            {
                var files = _currentFolder.ListFiles(new FileFilter(f => f.IsFile));
                var convFiles = files.Select(f => new AndroidFile(f)).ToList();
                IReadOnlyList<IFile> result = new ReadOnlyCollection<AndroidFile>(convFiles);
                return result;
            });
        }

        public Task<IReadOnlyList<IFile>> GetFilesAsync(uint startIndex, uint maxItemsToRetrieve)
        {
            throw new NotImplementedException();
        }

        public Task<IFolder> GetFolderAsync(string name)
        {
            return Task.Run(() =>
            {
                var file = new File(_currentFolder, name);
                if (file.Exists() && file.IsDirectory)
                {
                    IFolder result = new AndroidFolder(file);
                    return result;
                }

                throw new DirectoryNotFoundException(name);
            });
        }

        public Task<IReadOnlyList<IFolder>> GetFoldersAsync()
        {
            return Task.Run(() =>
            {
                var folders = _currentFolder.ListFiles(new FileFilter(f => f.IsDirectory));
                var convFiles = folders.Select(f => new AndroidFolder(f)).ToList();
                IReadOnlyList<IFolder> result = new ReadOnlyCollection<AndroidFolder>(convFiles);
                return result;
            });
        }

        public void Dispose()
        {
            _currentFolder.Dispose();
        }

        private sealed class FileFilter : Java.Lang.Object, IFileFilter
        {
            private readonly Func<File, bool> _comparator;

            public FileFilter(Func<File, bool> comparator)
            {
                _comparator = comparator;
            }

            public bool Accept(File pathname) => _comparator(pathname);
        }
    }
}