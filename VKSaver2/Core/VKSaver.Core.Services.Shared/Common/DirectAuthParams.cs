using ModernDev.InTouch;
using System;
using System.Collections.Generic;
using System.Text;

namespace VKSaver.Core.Services.Common
{
    public sealed class DirectAuthParams : MethodParamsGroup
    {
        [MethodParam(Name = "grant_type")]
        public string GrantType { get { return "password"; } }

        [MethodParam(Name = "client_id")]
        public int ClientId { get; set; }
    }
}
