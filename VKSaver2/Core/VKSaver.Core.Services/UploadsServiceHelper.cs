using System.Threading.Tasks;
using VKSaver.Core.Models.Transfer;
using VKSaver.Core.Services.Interfaces;

namespace VKSaver.Core.Services
{
    public sealed class UploadsServiceHelper : IUploadsServiceHelper
    {
        public UploadsServiceHelper(
            IUploadsPreprocessor uploadsPreprocessor,
            IUploadsService uploadsService,
            ILocService locService,
            IDialogsService dialogsService)
        {
            _uploadsPreprocessor = uploadsPreprocessor;
            _uploadsService = uploadsService;
            _locService = locService;
            _dialogsService = dialogsService;
        }

        public async Task<bool> StartUploadingAsync(IUploadable item)
        {
            var preResult = await _uploadsPreprocessor.ProcessUploadableAsync(item);
            if (preResult.Item1 == null)
            {
                ShowPreprocessionError(item, preResult.Item2);
                return false;
            }

            var error = await _uploadsService.StartUploadingAsync(preResult.Item1);
            if (error == null)
                return true;

            ShowInitError(error);
            return false;
        }
        
        private void ShowPreprocessionError(IUploadable upload, UploadsPreprocessorResultType error)
        {
            string text = null;
            switch (error)
            {
                case UploadsPreprocessorResultType.ConnectionError:
                    text = _locService["Message_Uploads_PreprocessionConnectionError_Text"];
                    break;
                case UploadsPreprocessorResultType.ServerError:
                    text = _locService["Message_Uploads_PreprocessionServerError_Text"];
                    break;
                default:
                    text = _locService["Message_Uploads_UnknownError_Text"];
                    break;
            }

            _dialogsService.Show($"{text}\n{upload.Name}",
                _locService["Message_Uploads_PreprocessionError_Title"]);
        }

        private void ShowInitError(UploadInitError error)
        {
            string text = null;
            switch (error.ErrorType)
            {
                case UploadInitErrorType.CantPrepareData:
                    text = _locService["Message_Uploads_CantPrepareData_Text"];
                    break;
                case UploadInitErrorType.MaxUploadsExceeded:
                    text = _locService["Message_Uploads_MaxUploadsExceeded_Text"];
                    break;
                default:
                    text = _locService["Message_Uploads_UnknownError_Text"];
                    break;
            }

            _dialogsService.Show($"{text}\n{error.Upload.Uploadable.Name}",
                _locService["Message_Uploads_StartUploading_Title"]);
        }

        private readonly IUploadsPreprocessor _uploadsPreprocessor;
        private readonly IUploadsService _uploadsService;
        private readonly ILocService _locService;
        private readonly IDialogsService _dialogsService;
    }
}
