using OneTeam.SDK.VK.Models.Common;
using System;
using System.Globalization;
using VKSaver.Core.Services.Interfaces;
using Windows.ApplicationModel.Resources;

namespace VKSaver.Core.Services
{
    public sealed class LocService : ILocService
    {
        public LocService()
        {
            _loader = ResourceLoader.GetForViewIndependentUse();
        }

        public string this[string resName] { get { return GetResource(resName); } }

        public string GetResource(string resName)
        {
            return _loader.GetString(resName);
        }

        public Language GetLanguage()
        {
            string currentLanguage = CultureInfo.CurrentCulture.Name;
            Language lang = Language.en;
            Enum.TryParse(currentLanguage, out lang);

            return lang;
        }
        
        private readonly ResourceLoader _loader;
    }
}
