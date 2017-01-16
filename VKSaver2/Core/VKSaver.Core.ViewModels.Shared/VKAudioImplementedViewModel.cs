#if WINDOWS_UWP
using Prism.Windows.Mvvm;
using Prism.Commands;
using Prism.Windows.Navigation;
#else
using Microsoft.Practices.Prism.StoreApps.Interfaces;
#endif

using ModernDev.InTouch;
using PropertyChanged;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.Models.Player;
using VKSaver.Core.ViewModels.Common;
using VKSaver.Core.Models.Transfer;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public abstract class VKAudioImplementedViewModel : VKAudioViewModel<Audio>
    {
        protected VKAudioImplementedViewModel(
            InTouch inTouch, 
            IAppLoaderService appLoaderService, 
            IDialogsService dialogsService, 
            IInTouchWrapper inTouchWrapper, 
            IDownloadsServiceHelper downloadsServiceHelper, 
            IPlayerService playerService,
            ILocService locService, 
            INavigationService navigationService)
            : base(inTouch, appLoaderService, dialogsService, inTouchWrapper, downloadsServiceHelper, playerService, locService, navigationService)
        {

        }

        protected override IPlayerTrack ConvertToPlayerTrack(Audio track)
        {
            return track.ToPlayerTrack();
        }

        protected override IDownloadable ConvertToDownloadable(Audio track)
        {
            return track.ToDownloadable();
        }

        protected override VKAudioInfo GetAudioInfo(Audio track)
        {
            return new VKAudioInfo(track.Id, track.OwnerId);
        }
    }
}
