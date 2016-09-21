using System;
using VKSaver.Core.Services.Common;
using VKSaver.Core.Services.Interfaces;
using Windows.UI.Core;
using Windows.UI.Popups;

namespace VKSaver.Core.Services
{
    public class DialogsService : IDialogsService
    {
        public DialogsService(IDispatcherWrapper dispatcherWrapper)
        {
            _dispatcherWrapper = dispatcherWrapper;
            _queue = new TaskQueue();
        }

        /// <summary>
        /// Отобразить сообщение с указанным заголовком и сообщением.
        /// </summary>
        /// <param name="message">Текст сообщения.</param>
        /// <param name="title">Заголовок собщения.</param>
        public async void Show(string message, string title = "")
        {
            await _dispatcherWrapper.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                var msg = new MessageDialog(message, title);
                await _queue.Enqueue(() => msg.ShowAsync().AsTask());
            });
        }

        private readonly TaskQueue _queue;
        private readonly IDispatcherWrapper _dispatcherWrapper;
    }
}
