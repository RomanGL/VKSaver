using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VKSaver.Core.Models.Common;

namespace VKSaver.Core.Services.Common
{
    public sealed class LFAuthorization : IServiceAuthorization
    {
        internal LFAuthorization() { }

        public bool IsAuthorized { get; internal set; }

        public string ServiceName { get { return "last.fm"; } }

        public Action SignInMethod { get; internal set; }

        public Action SignOutMethod { get; internal set; }

        public string UserName { get; internal set; }
    }
}
