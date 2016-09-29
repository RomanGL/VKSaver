using System.Collections.Generic;
using VKSaver.Core.Models.Common;

namespace VKSaver.Core.Services.Interfaces
{
    public interface ILocService
    {
        string this[string resName] { get; }
        AppLanguage CurrentAppLanguage { get; set; }
        string CurrentLanguage { get; }

        List<LanguageItem> GetAvailableLanguages();
        void ApplyAppLanguage();
        string GetResource(string resName);
    }
}
