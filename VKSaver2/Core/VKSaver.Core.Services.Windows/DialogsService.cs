using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VKSaver.Core.Services.Common;
using VKSaver.Core.Services.Interfaces;
using Windows.UI.Core;
using Windows.UI.Popups;

namespace VKSaver.Core.Services
{
    public class DialogsService : IDialogsService
    {
        public DialogsService(IDispatcherWrapper dispatcherWrapper,
            ILocService locService)
        {
            _dispatcherWrapper = dispatcherWrapper;
            _locService = locService;
            _queue = new TaskQueue();
        }

        /// <summary>
        /// Отобразить сообщение с указанным заголовком и сообщением.
        /// </summary>
        /// <param name="message">Текст сообщения.</param>
        /// <param name="title">Заголовок собщения.</param>
        public async void Show(string message, string title = "")
        {
            await _dispatcherWrapper.RunOnUIThreadAsync(async () =>
            {
                var msg = new MessageDialog(message, title);
                await _queue.Enqueue(() => msg.ShowAsync().AsTask());
            });
        }

        public async Task<bool> ShowYesNoAsync(string message, string title = "")
        {
            var msg = new MessageDialog(message, title);
            msg.Commands.Add(new UICommand(_locService["Message_YesButton_Text"]) { Id = "yes" });
            msg.Commands.Add(new UICommand(_locService["Message_NoButton_Text"]) { Id = "no" });

            msg.DefaultCommandIndex = 0;
            msg.CancelCommandIndex = 1;

            var result = await _queue.Enqueue(() => msg.ShowAsync().AsTask());
            return ((string)result.Id) == "yes";
        }

        private readonly TaskQueue _queue;
        private readonly IDispatcherWrapper _dispatcherWrapper;
        private readonly ILocService _locService;
    }
}
