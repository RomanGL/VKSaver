namespace VKSaver.Core.Services
{
    public static class AppConstants
    {
        public const string DEFAULT_LOGIN_VIEW = "LoginView";

        public const string CURRENT_PROMO_INDEX_PARAMETER = "CurrentPromoIndex";
        public const int CURRENT_PROMO_INDEX = 2;   // Текущий (последний) индекс промо-элементов.
        public const int DEFAULT_PROMO_INDEX = 1;   // Начальный индекс промо-элементов.

        #region Количество элементов в промо-индексах

        public const int COUNT_OF_PROMO_INDEX_2 = 4;

        #endregion

        public const string DEFAULT_MAIN_VIEW_PARAMETER = "DefaultMainView";
        public const string DEFAULT_MAIN_VIEW = "VKAdInfoView"; //"MainView";

        public const int CONNECTION_ERROR_CODE = -1;

        public const string CURRENT_LIBRARY_INDEX_PARAMETER = "CurrentLibraryIndex";
        public const int CURRENT_LIBRARY_INDEX = 1;

        public const string LAST_MUSIC_CHANGING_DATE = "LastMusicChangingDate";

        public const string CURRENT_FIRST_START_INDEX_PARAMETER = "CurrentFirstStartIndex";
        public const int CURRENT_FIRST_START_INDEX = 2;

        public const string CURRENT_FIRST_START_VIEW_PARAMETER = "CurrentFirstStartView";

        public const string INTERNET_ACCESS_TYPE = "InternetAccessType";
        public const string DOWNLOADS_NOTIFICATIONS_PARAMETER = "DownloadsNotifications";

        public const string SUCCESS_TOAST_SOUND = "ms-appx:///Assets/Sounds/Success.wav";
        public const string FAIL_TOAST_SOUND = "ms-appx:///Assets/Sounds/Fail.wav";
        public const string DEFAULT_PLAYER_BACKGROUND_IMAGE = "ms-appx:///Assets/Background/PlayerBackground.png";
                
        public const string SETTINGS_VERSION_PARAMETER = "SettingsVer";
        public const int SETTINGS_VERSION = 1;

        public const string ENABLE_IN_APP_SOUND = "EnableInAppSound";
        public const string ENABLE_IN_APP_VIBRATION = "EnableInAppVibration";
        public const string ENABLE_IN_APP_POPUPS = "EnableInAppPopup";

#if WINDOWS_UWP
        public const string YANDEX_METRICA_API_KEY = "***REMOVED***";
#else
        public const string YANDEX_METRICA_API_KEY = "***REMOVED***";
#endif
    }
}
