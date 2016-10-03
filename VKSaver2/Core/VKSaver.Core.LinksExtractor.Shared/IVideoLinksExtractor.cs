using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VKSaver.Core.LinksExtractor
{
    public interface IVideoLinksExtractor
    {
        Task<List<IVideoLink>> GetLinks(string videoUrl);
    }
}
