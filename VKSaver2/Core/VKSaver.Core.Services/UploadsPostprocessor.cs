using ModernDev.InTouch;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Models.Transfer;
using VKSaver.Core.Services.Interfaces;

namespace VKSaver.Core.Services
{
    public sealed class UploadsPostprocessor : IUploadsPostprocessor
    {
        public UploadsPostprocessor(
            InTouch inTouch, 
            IInTouchWrapper inTouchWrapper, 
            ILogService logService,
            IDialogsService dialogsService)
        {
            _inTouch = inTouch;
            _inTouchWrapper = inTouchWrapper;
            _logService = logService;
            _dialogsService = dialogsService;
        }

        public async Task<UploadsPostprocessorResultType> ProcessUploadAsync(ICompletedUpload upload)
        {
            Tuple<UploadsPostprocessorResultType, int> result = null;

            try
            {
                switch (upload.ContentType)
                {
                    case FileContentType.Music:
                        result = await SaveAudioAsync(upload.ServerResponse);
                        break;
                    case FileContentType.Video:
                        return UploadsPostprocessorResultType.Unknown;
                    default:
                        result = await SaveDocumentAsync(upload.ServerResponse);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logService.LogException(ex);
                if (ex.InnerException != null && ex.InnerException is HttpRequestException)
                {
                    result = new Tuple<UploadsPostprocessorResultType, int>(
                        UploadsPostprocessorResultType.ConnectionError, 0);
                }
            }

            if (result.Item1 == UploadsPostprocessorResultType.Success)
            {

            }
            else
                ShowError(upload, result.Item1, result.Item2);

            return result.Item1;
        } 
        
        private async Task<Tuple<UploadsPostprocessorResultType, int>> SaveAudioAsync(string serverResponse)
        {
            var json = JObject.Parse(serverResponse);

            var response = await _inTouchWrapper.ExecuteRequest(_inTouch.Audio.Save(new AudioUploadResponse
            {
                Server = json["server"].Value<string>(),
                Audio = json["audio"].Value<string>(),
                Hash = json["hash"].Value<string>()
            }));

            if (response.IsError)
            {
                return new Tuple<UploadsPostprocessorResultType, int>(
                    UploadsPostprocessorResultType.ServerError, response.Error.Code);
            }

            return new Tuple<UploadsPostprocessorResultType, int>(
                UploadsPostprocessorResultType.Success, 0);
        }

        private async Task<Tuple<UploadsPostprocessorResultType, int>> SaveDocumentAsync(string serverResponse)
        {
            var json = JObject.Parse(serverResponse);

            var response = await _inTouchWrapper.ExecuteRequest(_inTouch.Docs.Save(
                json["file"].Value<string>()));

            if (response.IsError)
            {
                return new Tuple<UploadsPostprocessorResultType, int>(
                    UploadsPostprocessorResultType.ServerError, response.Error.Code);
            }

            return new Tuple<UploadsPostprocessorResultType, int>(
                UploadsPostprocessorResultType.Success, 0);
        }

        private void ShowError(ICompletedUpload upload, UploadsPostprocessorResultType result, int code)
        {
            string text = null;

            if (result == UploadsPostprocessorResultType.ServerError)
            {
                text = String.Format(_locService["Message_PostUploads_ServerError_Text"],
                    upload.Name, code, GetServerErrorDescription(code));
            }
            else if (result == UploadsPostprocessorResultType.ConnectionError)
            {
                text = String.Format(_locService["Message_PostUploads_ConnectionError_Text"],
                    upload.Name);
            }
            else
            {
                text = String.Format(_locService["Message_PostUploads_UnknownAppError_Text"],
                    upload.Name);
            }

            _dialogsService.Show(text, _locService["Message_PostUploads_Error_Title"]);
        }

        private string GetServerErrorDescription(int code)
        {
            switch (code)
            {
                case 105:
                    return _locService["Message_PostUploads_ServerError_105_Text"];
                case 123:
                    return _locService["Message_PostUploads_ServerError_123_Text"];
                case 270:
                    return _locService["Message_PostUploads_ServerError_270_Text"];
                case 301:
                    return _locService["Message_PostUploads_ServerError_301_Text"];
                case 302:
                    return _locService["Message_PostUploads_ServerError_302_Text"];                
                default:
                    return _locService["Message_PostUploads_ServerError_Unknown_Text"];
            }
        }

        private readonly ILogService _logService;
        private readonly InTouch _inTouch;
        private readonly IInTouchWrapper _inTouchWrapper;
        private readonly IDialogsService _dialogsService;
        private readonly ILocService _locService;
    }
}
