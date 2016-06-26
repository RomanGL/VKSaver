using OneTeam.SDK.VK.Services.Interfaces;

namespace VKSaver.Core.Services.Interfaces
{
    public interface ILocService : ILanguageProvider
    {
        string this[string resName] { get; }

        string GetResource(string resName);
    }
}
