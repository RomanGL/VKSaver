using ModernDev.InTouch;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Models.Transfer;
using VKSaver.Core.Services.Interfaces;

namespace VKSaver.Core.Services
{
    public sealed class UploadsPreprocessor : IUploadsPreprocessor
    {
        public UploadsPreprocessor(
            InTouch inTouch,
            IInTouchWrapper inTouchWrapper,
            ILocService locService,
            IDialogsService dialogsService)
        {
            _inTouch = inTouch;
            _inTouchWrapper = inTouchWrapper;
            _locService = locService;
            _dialogsService = dialogsService;
        }

        public async Task<Tuple<IUpload, UploadsPreprocessorResultType>> ProcessUploadableAsync(IUploadable uploadable)
        {
            var result = await GetServerInfoForTypeAsync(uploadable);
            if (result.Result != UploadsPreprocessorResultType.Success)
            {
                ShowError(uploadable, result.Result, result.ErrorCode);
                return new Tuple<IUpload, UploadsPreprocessorResultType>(null, result.Result);
            }

            return new Tuple<IUpload, UploadsPreprocessorResultType>(new Upload
            {
                Uploadable = uploadable,
                UploadUrl = result.Info.UploadUrl
            }, UploadsPreprocessorResultType.Success);
        }

        private async Task<GetServerInfoResult> GetServerInfoForTypeAsync(IUploadable uploadable)
        {
            try
            {
                Response<ServerInfo> response = null;
                switch (uploadable.ContentType)
                {
                    case FileContentType.Music:
                        response = await _inTouchWrapper.ExecuteRequest(_inTouch.Audio.GetUploadServer());
                        break;
                    case FileContentType.Video:
                        return await GetServerInfoForVideoAsync(uploadable);
                    default:
                        response = await _inTouchWrapper.ExecuteRequest(_inTouch.Docs.GetUploadServer());
                        break;
                }

                if (response.IsError)
                {
                    return new GetServerInfoResult
                    {
                        Result = UploadsPreprocessorResultType.ServerError,
                        ErrorCode = response.Error.Code
                    };
                }

                return new GetServerInfoResult
                {
                    Result = UploadsPreprocessorResultType.Success,
                    Info = response.Data
                };
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null && ex.InnerException is HttpRequestException)
                {
                    return new GetServerInfoResult
                    {
                        Result = UploadsPreprocessorResultType.ConnectionError
                    };
                }
                else
                {
                    return new GetServerInfoResult
                    {
                        Result = UploadsPreprocessorResultType.UnknownError
                    };
                }
            }
        }

        private async Task<GetServerInfoResult> GetServerInfoForVideoAsync(IUploadable uploadable)
        {
            var vidResponse = await _inTouchWrapper.ExecuteRequest(_inTouch.Videos.Save(
                            new VideoSaveParams
                            {
                                Name = uploadable.AdditionalData["name"],
                                Description = uploadable.AdditionalData["description"],
                                PublishToWall = uploadable.AdditionalData["wallpost"] == "true",
                                Repeat = uploadable.AdditionalData["repeat"] == "true"
                            }));

            if (vidResponse.IsError)
            {
                return new GetServerInfoResult
                {
                    Result = UploadsPreprocessorResultType.ServerError,
                    ErrorCode = vidResponse.Error.Code
                };
            }
            else
            {
                return new GetServerInfoResult
                {
                    Result = UploadsPreprocessorResultType.Success,
                    Info = vidResponse.Data
                };
            }
        }

        private void ShowError(IUploadable uploadable, UploadsPreprocessorResultType error, int errorCode)
        {
            string text = null;

            if (error == UploadsPreprocessorResultType.ServerError)
            {
                text = String.Format(_locService["Message_Uploads_PreprocessionServerError_Text"],
                    uploadable.Name, errorCode, GetServerErrorDescription(errorCode));
            }
            else if (error == UploadsPreprocessorResultType.ConnectionError)
            {
                text = String.Format(_locService["Message_Uploads_PreprocessionConnectionError_Text"],
                    uploadable.Name);
            }
            else
            {
                text = String.Format(_locService["Message_Uploads_PreprocessionUnknownError_Text"],
                    uploadable.Name);
            }

            _dialogsService.Show(text, _locService["Message_Uploads_PreprocessionError_Title"]);
        }

        private string GetServerErrorDescription(int code)
        {
            switch (code)
            {
                case 204:
                    return _locService["Message_PostUploads_ServerError_204_Text"];
                case 214:
                    return _locService["Message_PostUploads_ServerError_214_Text"];
                default:
                    return _locService["Message_PostUploads_ServerError_Unknown_Text"];
            }
        }

        private readonly InTouch _inTouch;
        private readonly IInTouchWrapper _inTouchWrapper;
        private readonly ILocService _locService;
        private readonly IDialogsService _dialogsService;

        private sealed class GetServerInfoResult
        {
            public UploadsPreprocessorResultType Result { get; set; }
            public ServerInfo Info { get; set; }
            public int ErrorCode { get; set; }
        }
    }
}
