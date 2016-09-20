using System;
using VKSaver.Core.Services.Interfaces;
using Windows.UI.Popups;

namespace VKSaver.Core.Services
{
    public class DialogsService : IDialogsService
    {
        /// <summary>
        /// Отобразить сообщение с указанным заголовком и сообщением.
        /// </summary>
        /// <param name="message">Текст сообщения.</param>
        /// <param name="title">Заголовок собщения.</param>
        public void Show(string message, string title = "")
        {
            lock (_lockObject)
            {
                var msg = new MessageDialog(message, title);
                msg.ShowAsync().GetAwaiter().GetResult();
            }
        }

        private readonly object _lockObject = new object();
    }
}
