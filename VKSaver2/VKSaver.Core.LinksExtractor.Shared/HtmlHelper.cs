using HtmlAgilityPack;
using System;
using System.Threading.Tasks;
using Windows.Web.Http;

namespace VKSaver.Core.LinksExtractor
{
    internal static class HtmlHelper
    {
        private static HttpClient client;

        /// <summary>
        /// Статический конструктор.
        /// </summary>
        static HtmlHelper()
        {
            client = new HttpClient();
            client.DefaultRequestHeaders["User-Agent"] = "Mozilla/5.0 (compatible; MSIE 11.0; Windows Phone 8.1; Trident/6.0; IEMobile/11.0; ARM; Touch; NOKIA; Lumia 930)";
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
