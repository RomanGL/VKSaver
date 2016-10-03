using ModernDev.InTouch;
using VKSaver.Core.Models.Common;

namespace VKSaver.Core.Models.Extensions
{
    public static class CommonExtensions
    {
        public static string ToLangString(this AppLanguage lang)
        {
            switch (lang)
            {
                case AppLanguage.Russian:
                    return "ru";
                case AppLanguage.English:
                    return "en";
                case AppLanguage.Ukrainian:
                    return "uk";
                case AppLanguage.Belorussian:
                    return "be";
                case AppLanguage.German:
                    return "de";
                default:
                    return "en";
            }
        }

        public static Langs ToInTouchLang(this string langCode)
        {
            switch (langCode)
            {
                case "ru":
                    return Langs.Russian;
                case "be":
                    return Langs.Belorussian;
                case "uk":
                    return Langs.Ukrainian;
                case "de":
                    return Langs.German;
                default:
                    return Langs.English;
            }
        }

        public static Langs ToInTouchLang(this AppLanguage lang)
        {
            switch (lang)
            {
                case AppLanguage.Russian:
                    return Langs.Russian;
                case AppLanguage.English:
                    return Langs.English;
                case AppLanguage.Ukrainian:
                    return Langs.Ukrainian;
                case AppLanguage.Belorussian:
                    return Langs.Belorussian;
                case AppLanguage.German:
                    return Langs.German;
                default:
                    return Langs.English;
            }
        }

        public static string ToLastFmLang(this string langCode)
        {
            switch (langCode)
            {
                case "en":
                case "de":
                case "ru":
                    return langCode;
                case "uk":
                case "be":
                    return "ru";
                default:
                    return "en";
            }
        }
    }
}
