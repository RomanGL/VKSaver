namespace VKSaver.Core.Services.Interfaces
{
    public interface ILocService
    {
        string this[string resName] { get; }

        string GetResource(string resName);
    }
}
