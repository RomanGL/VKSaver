using Newtonsoft.Json;
using Prism.Commands;
using Prism.Windows.Mvvm;
using Prism.Windows.Navigation;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class HamburgerMenuViewModel : ViewModelBase
    {
        public HamburgerMenuViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            ItemClickCommand = new DelegateCommand<string>(OnItemClickCommand);
        }

        public bool IsPaneOpen { get; set; }

        public int DisplayMode { get; set; }

        [DoNotNotify]
        public DelegateCommand<string> ItemClickCommand { get; private set; }

        private void OnItemClickCommand(string tag)
        {
            switch (tag)
            {
                case "main":
                    _navigationService.Navigate("MainView", null);
                    break;
                case "audios":
                case "videos":
                case "docs":
                    _navigationService.Navigate("UserContentView", JsonConvert.SerializeObject(
                        new KeyValuePair<string, string>(tag, "0")));
                    break;
                case "settings":
                    _navigationService.Navigate("SettingsView", null);
                    break;
            }

            if (DisplayMode != 3)
                IsPaneOpen = false;
        }

        private readonly INavigationService _navigationService;
    }
}
