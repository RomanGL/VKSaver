using SQLite.Net.Interop;
using SQLite.Net.Platform.WinRT;
using Windows.Storage;

namespace VKSaver.Core.Services.Database
{
    public class SQLitePlatformZ : ISQLitePlatform
    {
        public SQLitePlatformZ()
        {
            SQLiteApi = new SQLiteApiZ(DatabaseRootDirectory);
            VolatileService = new VolatileServiceWinRT();
            StopwatchFactory = new StopwatchFactoryWinRT();
            ReflectionService = new ReflectionServiceWinRT();
        }

        public string DatabaseRootDirectory
        {
            get { return ApplicationData.Current.LocalFolder.Path; }
        }

        public ISQLiteApi SQLiteApi { get; private set; }
        public IStopwatchFactory StopwatchFactory { get; private set; }
        public IReflectionService ReflectionService { get; private set; }
        public IVolatileService VolatileService { get; private set; }
    }
}
