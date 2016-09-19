using System;
using System.ComponentModel;
using VKSaver.Core.Models.Common;

namespace VKSaver.Core.Services.Common
{
    public sealed class VKAuthorization : IServiceAuthorization, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        internal VKAuthorization()
        {
        }

        public bool IsAuthorized { get; internal set; }

        public string ServiceName { get { return "vk.com"; } }

        public Action SignInMethod { get; internal set; }

        public Action SignOutMethod { get; internal set; }

        public string UserName
        {
            get { return _userName; }
            set
            {
                if (value != _userName)
                {
                    _userName = value;
                    OnPropertyChanged(nameof(UserName));
                }
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _userName;
    }
}
