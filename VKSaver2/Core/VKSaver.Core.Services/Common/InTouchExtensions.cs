using ModernDev.InTouch;

namespace VKSaver.Core.Services.Common
{
    public static class InTouchExtensions
    {
        public static bool IsCaptchaError<T>(this Response<T> resp)
        {
            return resp.IsError && resp.Error.Code == 14;
        }
    }
}
