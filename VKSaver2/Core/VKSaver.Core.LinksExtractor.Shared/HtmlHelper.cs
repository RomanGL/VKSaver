using HtmlAgilityPack;
using System;
using System.Threading.Tasks;

#if ANDROID
using System.Net.Http;
#else
using Windows.Web.Http;
#endif

namespace VKSaver.Core.LinksExtractor
{
    internal static class HtmlHelper
    {
        private const string USER_AGENT = "Mozilla/5.0 (compatible; MSIE 11.0; Windows Phone 8.1; Trident/6.0; IEMobile/11.0; ARM; Touch; NOKIA; Lumia 930)";
        private static HttpClient client;

        /// <summary>
        /// Статический конструктор.
        /// </summary>
        static HtmlHelper()
        {
            client = new HttpClient();

#if ANDROID
            client.DefaultRequestHeaders.Add("User-Agent", USER_AGENT);
#else
            client.DefaultRequestHeaders["User-Agent"] = USER_AGENT;
#endif
        }

        /// <summary>
        /// Возвращает <see cref="HtmlDocument"/>, загруженный по указанной ссылке.
        /// </summary>
        /// <param name="url">Ссылка для загрузки.</param>
        public static async Task<HtmlDocument> GetHtmlDocumentFromUrl(string url)
        {
            var document = new HtmlDocument();
            document.LoadHtml(await client.GetStringAsync(new Uri(url)));

            return document;
        }
    }
}
