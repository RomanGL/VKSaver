using System;
using VKSaver.Core.Models.Common;

namespace VKSaver.Core.Services.Common
{
    public sealed class VKAuthorization : IServiceAuthorization
    {
        internal VKAuthorization()
        {
        }

        public bool IsAuthorized { get; internal set; }

        public string ServiceName { get { return "vk.com"; } }

        public Action SignInMethod { get; internal set; }

        public Action SignOutMethod { get; internal set; }

        public string UserName { get; internal set; }
    }
}
