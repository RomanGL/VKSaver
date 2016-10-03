using System;

namespace VKSaver.Core.Models.Common
{
    /// <summary>
    /// Представляет информацию об авторизации в веб-сервисе.
    /// </summary>
    public interface IServiceAuthorization
    {
        string ServiceName { get; }        
        string UserName { get; }

        bool IsAuthorized { get; }

        Action SignInMethod { get; }
        Action SignOutMethod { get; }
    }
}
