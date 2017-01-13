using System;
using System.Threading.Tasks;
using VKSaver.Core.Services.Interfaces;
using Windows.ApplicationModel.Store;
using Windows.System;
using Windows.UI.Core;

namespace VKSaver.Core.Services
{
    public sealed class FeedbackService : IFeedbackService
    {
        public FeedbackService(
            ISettingsService settingsService,
            IDialogsService dialogsService,
            IDispatcherWrapper dispatcherWrapper,
            ILocService locService)
        {
            _settingsService = settingsService;
            _dialogsService = dialogsService;
            _dispatcherWrapper = dispatcherWrapper;
            _locService = locService;
        }

        public async void ActivateFeedbackNotifier()
        {
            await Task.Run(() => ShowRateDialog());            
        }

        private async Task ShowRateDialog()
        {
            int startNumber = _settingsService.Get(FEEDBACK_PARAMETER_NAME, 0);

            if (startNumber == -1)
                return;
            else if (startNumber < 10)
            {
                startNumber++;
                _settingsService.Set(FEEDBACK_PARAMETER_NAME, startNumber);
            }
            else
            {
                await _dispatcherWrapper.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    bool result = await _dialogsService.ShowYesNoAsync(
                        _locService["FeedbackService_LeaveFeedback_Content"], 
                        _locService["FeedbackService_LeaveFeedback_Title"]);

                    _settingsService.Set(FEEDBACK_PARAMETER_NAME, -1);

                    if (result)
                        await Launcher.LaunchUriAsync(new Uri("ms-windows-store://reviewapp/?AppId=" + CurrentApp.AppId));
                });
            }
        }

        private readonly ISettingsService _settingsService;
        private readonly IDialogsService _dialogsService;
        private readonly IDispatcherWrapper _dispatcherWrapper;
        private readonly ILocService _locService;

        private const string FEEDBACK_PARAMETER_NAME = "FeedbackTask";
    }
}
