using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Models.Transfer;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.ViewModels.Common;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Media.Imaging;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class UploadFileViewModel : ViewModelBase, IFileOpenPickerSupport
    {
        public UploadFileViewModel(
            IUploadsServiceHelper uploadsServiceHelper,
            INavigationService navigationService,
            IAppLoaderService appLoaderService)
        {
            _uploadsServiceHelper = uploadsServiceHelper;
            _navigationService = navigationService;
            _appLoaderService = appLoaderService;

            SelectFileCommand = new DelegateCommand(OnSelectFileCommand);
            UploadFileCommand = new DelegateCommand(OnUploadFileCommand, CanExecuteUploadFileCommand);
            SelectAnotherFileCommand = new DelegateCommand(OnSelectAnotherFileCommand);
        }

        [DoNotNotify]
        public UploadType SelectedUploadType
        {
            get { return _selectedUploadType; }
            set
            {
                if (value == _selectedUploadType)
                    return;

                _selectedUploadType = value;
                SelectedFile = null;
                OnPropertyChanged(nameof(SelectedUploadType));
            }
        }

        [AlsoNotifyFor(nameof(SelectedFile))]
        public bool IsFileSelected { get { return SelectedFile != null; } }

        public FileSize SelectedFileSize { get; private set; }

        public string SelectedFileType { get; private set; }

        public string SelectedFileName { get; private set; }

        public BitmapImage SelectedFileImage { get; private set; }
        
        [DoNotNotify]
        public DelegateCommand SelectFileCommand { get; private set; }
        
        [DoNotNotify]
        public DelegateCommand UploadFileCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand SelectAnotherFileCommand { get; private set; }
        
        private StorageFile SelectedFile { get; set; }

        public void StartFileOpenPicker()
        {
            var picker = new FileOpenPicker();

            switch (SelectedUploadType)
            {
                case UploadType.Audio:
                    picker.SuggestedStartLocation = PickerLocationId.MusicLibrary;
                    picker.FileTypeFilter.Add(".mp3");
                    break;
                case UploadType.Video:
                    picker.SuggestedStartLocation = PickerLocationId.VideosLibrary;
                    picker.FileTypeFilter.Add(".avi");
                    picker.FileTypeFilter.Add(".mp4");
                    picker.FileTypeFilter.Add(".3gp");
                    picker.FileTypeFilter.Add(".mpeg");
                    picker.FileTypeFilter.Add(".mov");
                    picker.FileTypeFilter.Add(".mp3");
                    picker.FileTypeFilter.Add(".flv");
                    picker.FileTypeFilter.Add(".wmv");
                    break;
                case UploadType.Document:
                    picker.FileTypeFilter.Add("*");
                    break;
            }

            picker.PickSingleFileAndContinue();
        }

        public async void ContinueFileOpenPicker(IReadOnlyList<StorageFile> files)
        {
            try
            {
                if (files != null && files.Count > 0)
                    SelectedFile = files[0];
                else
                {
                    SelectedFile = null;
                    SelectedFileImage = null;
                    return;
                }

                try
                {
                    var properties = await SelectedFile.GetBasicPropertiesAsync();
                    SelectedFileName = SelectedFile.DisplayName;
                    SelectedFileSize = FileSize.FromBytes(properties.Size);
                    SelectedFileType = SelectedFile.DisplayType;
                }
                catch (Exception)
                {
                    SelectedFile = null;
                    SelectedFileImage = null;
                    return;
                }

                try
                {
                    var img = await SelectedFile.GetThumbnailAsync(ThumbnailMode.SingleItem, 256);
                    SelectedFileImage = new BitmapImage();
                    await SelectedFileImage.SetSourceAsync(img);
                }
                catch (Exception)
                {
                    SelectedFileImage = null;
                }
            }
            finally
            {
                UploadFileCommand.RaiseCanExecuteChanged();
                _appLoaderService.Hide();
            }
        }

        private void OnSelectFileCommand()
        {
            _appLoaderService.Show();
            StartFileOpenPicker();
        }

        private async void OnUploadFileCommand()
        {
            _appLoaderService.Show();
            bool isSuccess = await _uploadsServiceHelper.StartUploadingAsync(new SimpleUploadable
            {
                ContentType = GetContentTypeFromUploadType(SelectedUploadType),
                Name = SelectedFileName,
                Extension = SelectedFile.FileType,
                Source = new StorageFileSource(SelectedFile)
            });
            _appLoaderService.Hide();

            if (isSuccess)
                _navigationService.Navigate("TransferView", "uploads");
        }

        private bool CanExecuteUploadFileCommand()
        {
            return IsFileSelected;
        }

        private void OnSelectAnotherFileCommand()
        {
            SelectedFile = null;
        }

        private static FileContentType GetContentTypeFromUploadType(UploadType type)
        {
            switch (type)
            {
                case UploadType.Audio:
                    return FileContentType.Music;
                case UploadType.Video:
                    return FileContentType.Video;
                default:
                    return FileContentType.Other;
            }
        }
        
        private UploadType _selectedUploadType;

        private readonly IUploadsServiceHelper _uploadsServiceHelper;
        private readonly INavigationService _navigationService;
        private readonly IAppLoaderService _appLoaderService;
    }
}
