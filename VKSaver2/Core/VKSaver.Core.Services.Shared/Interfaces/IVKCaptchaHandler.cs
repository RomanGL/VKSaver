using System.Threading.Tasks;

namespace VKSaver.Core.Services.Interfaces
{
    public interface IVKCaptchaHandler
    {
        Task<string> GetCaptchaUserInput(string captchaImg);
    }
}
