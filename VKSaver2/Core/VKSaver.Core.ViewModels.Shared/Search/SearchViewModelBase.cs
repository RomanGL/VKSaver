#if WINDOWS_UWP || WINDOWS_PHONE_APP
using Windows.System;
using Windows.UI.Xaml.Input;
#endif

#if WINDOWS_UWP
using Prism.Commands;
using Prism.Windows.Navigation;
#elif WINDOWS_PHONE_APP
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
#elif ANDROID
using VKSaver.Core.Toolkit.Commands;
using Android.Views.InputMethods;
using Android.Widget;
#endif

using ModernDev.InTouch;
using Newtonsoft.Json;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.Toolkit;
using VKSaver.Core.Toolkit.Controls;
using VKSaver.Core.ViewModels.Collections;
using VKSaver.Core.ViewModels.Search;
using VKSaver.Core.Toolkit.Navigation;
using NavigatedToEventArgs = VKSaver.Core.Toolkit.Navigation.NavigatedToEventArgs;
using NavigatingFromEventArgs = VKSaver.Core.Toolkit.Navigation.NavigatingFromEventArgs;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public abstract class SearchViewModelBase<T> : VKSaverViewModel
    {
        public SearchViewModelBase(InTouch inTouch, INavigationService navigationService,
            ILocService locService, ISettingsService settingsService, IDialogsService dialogsService,
            IInTouchWrapper inTouchWrapper)
        {
            _inTouch = inTouch;
            _navigationService = navigationService;
            _locService = locService;
            _settingsService = settingsService;
            _dialogsService = dialogsService;
            _inTouchWrapper = inTouchWrapper;

            PrimaryItems = new ObservableCollection<IButtonElement>();
            SecondaryItems = new ObservableCollection<IButtonElement>();
            SelectedItems = new List<object>();
            
            SelectionChangedCommand = new DelegateCommand(OnSelectionChangedCommand);
            ExecuteItemCommand = new DelegateCommand<T>(OnExecuteItemCommand);
            OpenTransferManagerCommand = new DelegateCommand(OnOpenTransferManagerCommand);
            ReloadCommand = new DelegateCommand(Search);
            ActivateSelectionModeCommand = new DelegateCommand(SetSelectionMode, CanActivateSelectionModeCommand);
            SelectAllCommand = new DelegateCommand(() => SelectAll = !SelectAll);

#if WINDOWS_UWP || WINDOWS_PHONE_APP
            QueryBoxKeyDownCommand = new DelegateCommand<KeyRoutedEventArgs>(OnQueryBoxKeyDownCommand);
#elif ANDROID
            QueryBoxActionCommand = new DelegateCommand<TextView.EditorActionEventArgs>(OnQueryBoxActionCommand);
#endif
        }

        public int LastPivotIndex
        {
            get { return _lastPivotIndex; }
            set
            {
                _lastPivotIndex = value;
                OnLastPivotIndexChanged();
            }
        }

        public bool IsLockedPivot { get; private set; }
        public bool IsSelectionMode { get; private set; }
        public bool IsItemClickEnabled { get; private set; }
        public bool SelectAll { get; private set; }
        public string Query { get; set; }

        public PaginatedCollection<T> EverywhereResults { get; private set; }
        public PaginatedCollection<T> InCollectionResults { get; private set; }

        [DoNotNotify]
        public ObservableCollection<IButtonElement> PrimaryItems { get; private set; }
        [DoNotNotify]
        public ObservableCollection<IButtonElement> SecondaryItems { get; private set; }
        [DoNotNotify]
        public List<object> SelectedItems { get; private set; }

#if WINDOWS_UWP || WINDOWS_PHONE_APP
        [DoNotNotify]
        public DelegateCommand<KeyRoutedEventArgs> QueryBoxKeyDownCommand { get; private set; }
#elif ANDROID
        [DoNotNotify]
        public DelegateCommand<TextView.EditorActionEventArgs> QueryBoxActionCommand { get; private set; }
#endif

        [DoNotNotify]
        public DelegateCommand SelectionChangedCommand { get; private set; }
        [DoNotNotify]
        public DelegateCommand<T> ExecuteItemCommand { get; private set; }
        [DoNotNotify]
        public DelegateCommand ReloadCommand { get; private set; }
        [DoNotNotify]
        public DelegateCommand ActivateSelectionModeCommand { get; private set; }
        [DoNotNotify]
        public DelegateCommand OpenTransferManagerCommand { get; private set; }
        [DoNotNotify]
        public DelegateCommand SelectAllCommand { get; private set; }

        protected int UserId { get; private set; }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            if (viewModelState.Count == 0)
            {
                if (e.Parameter != null)
                {
                    var parameter = JsonConvert.DeserializeObject<SearchNavigationParameter>(e.Parameter.ToString());
                    UserId = parameter.UserId;
                    LastPivotIndex = parameter.Section == SearchNavigationParameter.SearchSection.Ewerywhere ? 0 : 1;
                    Query = parameter.Query;
                }
                else
                {
                    UserId = _inTouch.Session.UserId;
                    LastPivotIndex = 0;
                }

                _everywhereOffset = 0;
                _inCollectionOffset = 0;

                EverywhereResults = new PaginatedCollection<T>(LoadMoreEverywhere);
                InCollectionResults = new PaginatedCollection<T>(LoadMoreInCollection);

                EverywhereResults.HasMoreItems = false;
                InCollectionResults.HasMoreItems = false;
                EverywhereResults.ContentState = ContentState.NoData;
                InCollectionResults.ContentState = ContentState.NoData;

                if (!String.IsNullOrWhiteSpace(Query))
                    Search();
            }
            else
            {
                LastPivotIndex = (int)viewModelState[nameof(LastPivotIndex)];
                Query = (string)viewModelState[nameof(Query)];
                UserId = (int)viewModelState[nameof(UserId)];
                _currentQuery = (string)viewModelState[nameof(_currentQuery)];
                _everywhereOffset = (int)viewModelState[nameof(_everywhereOffset)];
                _inCollectionOffset = (int)viewModelState[nameof(_inCollectionOffset)];

                EverywhereResults = JsonConvert.DeserializeObject<PaginatedCollection<T>>(
                    viewModelState[nameof(EverywhereResults)].ToString());
                InCollectionResults = JsonConvert.DeserializeObject<PaginatedCollection<T>>(
                    viewModelState[nameof(InCollectionResults)].ToString());

                EverywhereResults.LoadMoreItems = LoadMoreEverywhere;
                InCollectionResults.LoadMoreItems = LoadMoreInCollection;

                EverywhereResults.ContentState = (ContentState)(int)viewModelState[nameof(EverywhereResults) + "State"];
                InCollectionResults.ContentState = (ContentState)(int)viewModelState[nameof(InCollectionResults) + "State"];                

                if (EverywhereResults.ContentState == ContentState.NoData)
                    EverywhereResults.HasMoreItems = false;

                if (InCollectionResults.ContentState == ContentState.NoData)
                    InCollectionResults.HasMoreItems = false;
            }

            EverywhereResults.CollectionChanged += Results_CollectionChanged;
            InCollectionResults.CollectionChanged += Results_CollectionChanged;

            SetDefaultMode();
            base.OnNavigatedTo(e, viewModelState);
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
        {
            if (e.NavigationMode == NavigationMode.Back && IsSelectionMode)
            {
                SetDefaultMode();
                e.Cancel = true;
                return;
            }

            EverywhereResults.CollectionChanged -= Results_CollectionChanged;
            InCollectionResults.CollectionChanged -= Results_CollectionChanged;

            if (e.NavigationMode == NavigationMode.New)
            {
                viewModelState[nameof(LastPivotIndex)] = LastPivotIndex;
                viewModelState[nameof(Query)] = Query;
                viewModelState[nameof(UserId)] = UserId;
                viewModelState[nameof(_currentQuery)] = _currentQuery;
                viewModelState[nameof(_everywhereOffset)] = _everywhereOffset;
                viewModelState[nameof(_inCollectionOffset)] = _inCollectionOffset;
                viewModelState[nameof(EverywhereResults)] = JsonConvert.SerializeObject(EverywhereResults.ToList());
                viewModelState[nameof(InCollectionResults)] = JsonConvert.SerializeObject(InCollectionResults.ToList());
                viewModelState[nameof(EverywhereResults) + "State"] = (int)EverywhereResults.ContentState;
                viewModelState[nameof(InCollectionResults) + "State"] = (int)InCollectionResults.ContentState;
            }
            
            base.OnNavigatingFrom(e, viewModelState, suspending);
        }

        protected abstract Task<IEnumerable<T>> LoadMoreEverywhere(uint page);
        protected abstract Task<IEnumerable<T>> LoadMoreInCollection(uint page);
        
        protected virtual void CreateDefaultAppBarButtons()
        {
            PrimaryItems.Clear();
            SecondaryItems.Clear();

            PrimaryItems.Add(new ButtonElement
            {
                Label = _locService["AppBarButton_Refresh_Text"],
                Icon = new FontButtonIcon { Glyph = "\uE117", FontSize = 14 },
                Command = ReloadCommand
            });

            SecondaryItems.Add(new ButtonElement
            {
                Label = _locService["AppBarButton_TransferManager_Text"],
                Command = OpenTransferManagerCommand
            });
        }

        protected virtual void CreateSelectionAppBarButtons()
        {
            PrimaryItems.Clear();
            SecondaryItems.Clear();
        }

        protected virtual void OnLastPivotIndexChanged()
        {
            if (LastPivotIndex == 0 && EverywhereResults.ContentState == ContentState.None)
                EverywhereResults.Refresh();
            else if (LastPivotIndex == 1 && InCollectionResults.ContentState == ContentState.None)
                InCollectionResults.Refresh();

            ActivateSelectionModeCommand.RaiseCanExecuteChanged();
        }

        protected virtual void OnSelectionChangedCommand()
        {
        }

        protected virtual void OnExecuteItemCommand(T item)
        {
        }

        protected void SetSelectionMode()
        {
            IsSelectionMode = true;
            IsItemClickEnabled = false;

            CreateSelectionAppBarButtons();
        }

        protected void SetDefaultMode()
        {
            IsSelectionMode = false;
            IsItemClickEnabled = true;

            CreateDefaultAppBarButtons();
        }

        protected void Search()
        {
            _everywhereOffset = 0;
            _inCollectionOffset = 0;
            _currentQuery = Query;

            EverywhereResults.Clear();
            InCollectionResults.Clear();

            if (LastPivotIndex == 0)
            {
                EverywhereResults.Refresh();
                InCollectionResults.HasMoreItems = false;
                InCollectionResults.ContentState = ContentState.None;
            }
            else
            {
                InCollectionResults.Refresh();
                EverywhereResults.HasMoreItems = false;
                EverywhereResults.ContentState = ContentState.None;
            }
        }

#if WINDOWS_UWP || WINDOWS_PHONE_APP
        private void OnQueryBoxKeyDownCommand(KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
                Search();
        }
#elif ANDROID
        private void OnQueryBoxActionCommand(TextView.EditorActionEventArgs e)
        {
            if (e.ActionId == ImeAction.Search)
                Search();
        }
#endif

        private void OnOpenTransferManagerCommand()
        {
            _navigationService.Navigate("TransferView", "downloads");
        }

        private void Results_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ActivateSelectionModeCommand.RaiseCanExecuteChanged();
        }

        private bool CanActivateSelectionModeCommand()
        {
            if (LastPivotIndex == 0)
                return EverywhereResults.Any();
            else
                return InCollectionResults.Any();
        }

        protected int _everywhereOffset;
        protected int _inCollectionOffset;
        protected string _currentQuery;

        private int _lastPivotIndex;

        protected readonly InTouch _inTouch;
        protected readonly INavigationService _navigationService;
        protected readonly ILocService _locService;
        protected readonly ISettingsService _settingsService;
        protected readonly IDialogsService _dialogsService;
        protected readonly IInTouchWrapper _inTouchWrapper;
    }
}