namespace VKSaver.Core.Models.Common
{
    public enum DirectAuthResult : byte
    {
        UnknownError = 0,
        Success,
        TwoFaSms,
        TwoFaApp,
        Validation,
        ConnectionError
    }
}
