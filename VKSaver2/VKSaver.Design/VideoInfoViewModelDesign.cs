using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VKSaver.Design
{
    public sealed class VideoInfoViewModelDesign
    {
        public object Video
        {
            get
            {
                return new
                {
                    Title = "Заголовок видео",
                    Description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed sed augue lacinia, sagittis sem eget, porta ante."
                };
            }
        }

        public string VideoStoresOn
        {
            get { return "Это видео хранится на серверах ВКонтакте"; }
        }
    }
}
