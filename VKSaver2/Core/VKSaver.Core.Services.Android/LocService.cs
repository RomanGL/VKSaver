using System;
using System.Collections.Generic;
using Android.Content;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Services.Interfaces;

namespace VKSaver.Core.Services
{
    public sealed class LocService : ILocService
    {
        private readonly Context _context;

        public LocService(Context context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            _context = context;
        }

        public string this[string resName] => GetResource(resName);

        public AppLanguage CurrentAppLanguage { get; set; }

        public string CurrentLanguage { get; }

        public List<LanguageItem> GetAvailableLanguages()
        {
            throw new NotImplementedException();
        }

        public void ApplyAppLanguage()
        {
            throw new NotImplementedException();
        }

        public string GetResource(string resName)
        {
            int id = _context.Resources.GetIdentifier(resName, "string", _context.PackageName);
            return _context.Resources.GetString(id);
        }
    }
}