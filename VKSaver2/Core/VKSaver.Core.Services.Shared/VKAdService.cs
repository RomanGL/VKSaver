using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Store;
using ModernDev.InTouch;
using Newtonsoft.Json;
using VKSaver.Core.Models;
using VKSaver.Core.Services.Interfaces;
using static VKSaver.Core.Services.MetricaConstants;

namespace VKSaver.Core.Services
{
    public sealed class VKAdService : IVKAdService
    {
        public VKAdService(
            IInTouchWrapper inTouchWrapper, 
            InTouch inTouch,
            IMetricaService metricaService,
            ILogService logService)
        {
            _inTouchWrapper = inTouchWrapper;
            _inTouch = inTouch;
            _metricaService = metricaService;
            _logService = logService;
        }

        public async Task<VKAdData> GetAdDataAsync(string adId)
        {
            try
            {
                var response = await _inTouchWrapper.ExecuteRequest(
                    _inTouch.Wall.GetById(new List<string>(1) {adId}, true, 0));

                if (response.IsError)
                    return null;

                var post = response.Data.Items[0];
                var data = JsonConvert.DeserializeObject<VKAdData>(post.Text);
                var photo = (Photo)post.Attachments[0];

                data.Photo =
                    !String.IsNullOrEmpty(photo.Photo1280)
                        ? photo.Photo1280
                        : !String.IsNullOrEmpty(photo.Photo807)
                            ? photo.Photo807
                            : !String.IsNullOrEmpty(photo.Photo604)
                                ? photo.Photo604
                                : !String.IsNullOrEmpty(photo.Photo130)
                                    ? photo.Photo130
                                    : photo.Photo75;
                return data;
            }
            catch (Exception e)
            {
                _logService.LogException(e);
                return null;
            }
        }

        public Task ReportAdAsync(string adName, bool isSuccess)
        {
            return Task.Run(() =>
            {
                var dict = new Dictionary<string, string>(1);
                if (isSuccess)
                    dict[adName] = SUBSCRIBE_SUCCESS;
                else
                    dict[adName] = SUBSCRIBE_DENIED;

                _metricaService.LogEvent(VK_AD_ACTION_EVENT, JsonConvert.SerializeObject(dict));
            });
        }

        private readonly IInTouchWrapper _inTouchWrapper;
        private readonly ILogService _logService;
        private readonly IMetricaService _metricaService;
        private readonly InTouch _inTouch;
    }
}
