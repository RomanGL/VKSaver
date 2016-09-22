namespace VKSaver.Core.Services.Database
{
    public enum DatabaseUpdateStepType : byte
    {
        Started = 0,
        SearchingFiles,
        PreparingDatabase,
        SearchingArtists,
        SearchingAlbums,
        Completed
    }
}
