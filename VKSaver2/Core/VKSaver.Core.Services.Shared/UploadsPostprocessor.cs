using ModernDev.InTouch;
using Newtonsoft.Json.Linq;
using NotificationsExtensions.ToastContent;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using VKSaver.Core.Models;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Models.Transfer;
using VKSaver.Core.Services.Common;
using VKSaver.Core.Services.Interfaces;
using Windows.UI.Notifications;

namespace VKSaver.Core.Services
{
    public sealed class UploadsPostprocessor : IUploadsPostprocessor
    {
        public UploadsPostprocessor(
            InTouch inTouch, 
            IInTouchWrapper inTouchWrapper, 
            ILogService logService,
            IDialogsService dialogsService,
            ILocService locService,
            IAppNotificationsService appNotificationsService)
        {
            _inTouch = inTouch;
            _inTouchWrapper = inTouchWrapper;
            _logService = logService;
            _dialogsService = dialogsService;
            _locService = locService;
            _appNotificationsService = appNotificationsService;
        }

        public async Task<UploadsPostprocessorResultType> ProcessUploadAsync(ICompletedUpload upload)
        {
            PostprocessionResult result = null;

            try
            {
                switch (upload.ContentType)
                {
                    case FileContentType.Music:
                        result = await SaveAudioAsync(upload.ServerResponse);
                        break;
                    case FileContentType.Video:
                        result = SaveVideo(upload.ServerResponse);
                        break;
                    default:
                        result = await SaveDocumentAsync(upload.ServerResponse);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logService.LogException(ex);
                if (ex.InnerException != null && ex.InnerException is HttpRequestException)
                    result = new PostprocessionResult { Result = UploadsPostprocessorResultType.ConnectionError };
                else
                    result = new PostprocessionResult { Result = UploadsPostprocessorResultType.Unknown };
            }

            if (result.Result == UploadsPostprocessorResultType.Success)
            {
                _appNotificationsService.SendNotification(new AppNotification
                {
                    Title = _locService["Toast_PostUploads_Success_Text"],
                    Content = upload.Name,
                    Type = AppNotificationType.Info
                });

                //var successToast = ToastContentFactory.CreateToastText02();
                //successToast.Audio.Content = ToastAudioContent.IM;
                //successToast.TextHeading.Text = _locService["Toast_PostUploads_Success_Text"];
                //successToast.TextBodyWrap.Text = upload.Name;

                //var successXml = successToast.GetXml();
                //ToastAudioHelper.SetSuccessAudio(successXml);
                //var toast = new ToastNotification(successXml);

                //ToastNotificationManager.CreateToastNotifier().Show(toast);
            }
            else
                ShowError(upload, result);

            return result.Result;
        } 
        
        private async Task<PostprocessionResult> SaveAudioAsync(string serverResponse)
        {
            var json = JObject.Parse(serverResponse);

            var result = GetError(json);
            if (result != null)
                return result;

            var response = await _inTouchWrapper.ExecuteRequest(_inTouch.Audio.Save(new AudioUploadResponse
            {
                Server = json["server"].Value<string>(),
                Audio = json["audio"].Value<string>(),
                Hash = json["hash"].Value<string>()
            }));

            if (response.IsError)
            {
                return new PostprocessionResult
                {
                    Result = UploadsPostprocessorResultType.ServerError,
                    ErrorCode = response.Error.Code
                };
            }

            return new PostprocessionResult { Result = UploadsPostprocessorResultType.Success };
        }

        private PostprocessionResult SaveVideo(string serverResponse)
        {
            var json = JObject.Parse(serverResponse);

            var result = GetError(json);
            if (result != null)
                return result;

            return new PostprocessionResult { Result = UploadsPostprocessorResultType.Success };
        }

        private async Task<PostprocessionResult> SaveDocumentAsync(string serverResponse)
        {
            var json = JObject.Parse(serverResponse);

            var result = GetError(json);
            if (result != null)
                return result;

            var response = await _inTouchWrapper.ExecuteRequest(_inTouch.Docs.Save(
                json["file"].Value<string>()));

            if (response.IsError)
            {
                return new PostprocessionResult
                {
                    Result = UploadsPostprocessorResultType.ServerError,
                    ErrorCode = response.Error.Code
                };
            }

            return new PostprocessionResult { Result = UploadsPostprocessorResultType.Success };
        }

        private PostprocessionResult GetError(JObject obj)
        {
            JToken errorToken = null;
            obj.TryGetValue("error", out errorToken);

            if (errorToken == null)
                return null;

            return new PostprocessionResult
            {
                Result = UploadsPostprocessorResultType.ServerError,
                ErrorCode = -1,
                UploadErrorText = errorToken.Value<string>()
            };
        }

        private void ShowError(ICompletedUpload upload, PostprocessionResult result)
        {
            //var fail = ToastContentFactory.CreateToastText02();
            //fail.Audio.Content = ToastAudioContent.IM;
            //fail.TextHeading.Text = _locService["Toast_PostUploads_Fail_Text"];
            //fail.TextBodyWrap.Text = upload.Name;

            //var failXml = fail.GetXml();
            //ToastAudioHelper.SetFailAudio(failXml);

            //ToastNotificationManager.CreateToastNotifier().Show(new ToastNotification(failXml));

            _appNotificationsService.SendNotification(new AppNotification
            {
                Title = $"{_locService["Toast_PostUploads_Fail_Text"]} - {upload.Name}",
                Content = "Коснитесь для подробностей",
                Type = AppNotificationType.Error,
                Duration = TimeSpan.FromSeconds(15),
                ActionToDo = () =>
                {
                    string text = null;
                    if (result.Result == UploadsPostprocessorResultType.ServerError)
                    {
                        text = String.Format(_locService["Message_PostUploads_ServerError_Text"],
                            upload.Name,
                            result.ErrorCode,
                            GetServerErrorDescription(result.ErrorCode, result.UploadErrorText));
                    }
                    else if (result.Result == UploadsPostprocessorResultType.ConnectionError)
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
            });
        }

        private string GetServerErrorDescription(int errorCode, string errorText = null)
        {
            switch (errorCode)
            {
                case -1:
                    return errorText;
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
        private readonly IAppNotificationsService _appNotificationsService;

        private sealed class PostprocessionResult
        {
            public UploadsPostprocessorResultType Result { get; set; }
            public int ErrorCode { get; set; }
            public string UploadErrorText { get; set; }
        }
    }
}
