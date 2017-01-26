using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModernDev.InTouch;
using Newtonsoft.Json;
using VKSaver.Core.Models;
using VKSaver.Core.Services.Interfaces;

namespace VKSaver.Core.Services
{
    public sealed class VKAdService : IVKAdService
    {
        public VKAdService(
            IInTouchWrapper inTouchWrapper, 
            InTouch inTouch)
        {
            _inTouchWrapper = inTouchWrapper;
            _inTouch = inTouch;
        }

        public async Task<VKAdData> GetAdDataAsync(string adId)
        {
            try
            {
                var response = await _inTouchWrapper.ExecuteRequest(
                    _inTouch.Wall.GetById(new List<string>(1) {adId}, false, 0));

                if (response.IsError)
                    return null;

                var post = response.Data.Items[0];
                var data = JsonConvert.DeserializeObject<VKAdData>(post.Text);

                return data;
            }
            catch (Exception e)
            {
                var x = e.Data["json"];
                return null;
            }
        }

        public async Task ReportAdAsync(bool isSuccess)
        {
            
        }

        private readonly IInTouchWrapper _inTouchWrapper;
        private readonly InTouch _inTouch;
    }
}
