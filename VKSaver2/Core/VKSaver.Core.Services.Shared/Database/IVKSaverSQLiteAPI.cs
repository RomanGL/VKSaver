using SQLite.Net.Interop;

namespace VKSaver.Core.Services.Database
{
    public interface IVKSaverSQLiteAPI : ISQLiteApiExt
    {
        int SetLimit(IDbHandle db, int id, int value);
    }
}
