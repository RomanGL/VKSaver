using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoExtractor.Models;
using VideoExtractor.Vimeo;

namespace VKSaver.Core.LinksExtractor
{
    public class VideoLinksExtractor : IVideoLinksExtractor
    {
        public Task<List<IVideoLink>> GetLinks(string videoUrl)
        {
            if (videoUrl.Contains("vk.com"))
                return GetVKLinks(videoUrl);
            //else if (videoUrl.Contains("youtube"))
            //    return GetYouTubeLinks(videoUrl);
            else if (videoUrl.Contains("vimeo.com"))
                return GetVimeoLinks(videoUrl);
            else
                throw new UnsupportedServiceException("Unsupported service", videoUrl);
        }

        private async Task<List<IVideoLink>> GetVimeoLinks(string videoUrl)
        {
            try
            {
                int index = videoUrl.IndexOf("/video/");
                string id = videoUrl.Substring(index + 7).Split('?')[0];

                var vimeo = new VimeoVideo(id);
                var videos = (await vimeo.ExtractAsync()).Where(v => v.VideoType == VideoType.Mp4 || v.VideoType == VideoType.Mobile);
                var links = new List<IVideoLink>();

                foreach (var video in videos)
                {
                    links.Add(new CommonVideoLink
                    {
                        Name = $"{video.VideoType.ToString().ToUpper()} {video.Resolution}p",
                        Source = video.VideoUri.ToString()
                    });
                }

                return links;
            }
            catch (Exception)
            {
                throw new LinksExtractionFailedException("Vimeo", videoUrl);
            }
        }

        private async Task<List<IVideoLink>> GetYouTubeLinks(string videoUrl)
        {
            try
            {
                return null;
            }
            catch (Exception)
            {
                throw new LinksExtractionFailedException("YouTube", videoUrl);
            }
        }

        private async Task<List<IVideoLink>> GetVKLinks(string videoUrl)
        {
            try
            {
                var document = await HtmlHelper.GetHtmlDocumentFromUrl(videoUrl);
                var videoBlock = document.GetElementbyId("video");

                var result = new List<IVideoLink>();

                foreach (var source in videoBlock.ChildNodes.Where(x => x.Name == "source"))
                {
                    string url = source.Attributes.FirstOrDefault(a => a.Name == "src").Value;
                    string name = "unknown";

                    if (url.Contains(".1080.mp4")) name = "MP4 1080p";
                    else if (url.Contains(".720.mp4")) name = "MP4 720p";
                    else if (url.Contains(".480.mp4")) name = "MP4 480p";
                    else if (url.Contains(".360.mp4")) name = "MP4 360p";
                    else if (url.Contains(".240.mp4")) name = "MP4 240p";

                    result.Add(new CommonVideoLink { Source = url, Name = name });
                }

                return result;
            }
            catch (Exception)
            {
                throw new LinksExtractionFailedException("VK", videoUrl);
            }
        }
    }
}
