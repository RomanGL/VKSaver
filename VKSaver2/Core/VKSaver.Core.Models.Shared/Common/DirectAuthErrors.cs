namespace VKSaver.Core.Models.Common
{
    public enum DirectAuthErrors : byte
    {
        None = 0,
        need_captcha,
        need_validation,
        invalid_client,
        access_denied,
        connection_error,
        unknown_error
    }
}
