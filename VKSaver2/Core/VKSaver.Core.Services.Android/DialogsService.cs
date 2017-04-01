using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using VKSaver.Core.Services.Interfaces;

namespace VKSaver.Core.Services
{
    public sealed class DialogsService : IDialogsService
    {
        public void Show(string message, string title = "")
        {
            
        }

        public Task<bool> ShowYesNoAsync(string message, string title = "")
        {
            return Task.FromResult(true);
        }
    }
}