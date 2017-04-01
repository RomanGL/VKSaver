using System.Threading.Tasks;
using VKSaver.Core.Services.Interfaces;

namespace VKSaver.Core.Services
{
    public sealed class VKCaptchaHandler : IVKCaptchaHandler
    {
        public async Task<string> GetCaptchaUserInput(string captchaImg)
        {
            return null;
        }
    }
}