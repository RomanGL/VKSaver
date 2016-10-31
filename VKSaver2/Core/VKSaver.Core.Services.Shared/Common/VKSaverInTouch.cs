using ModernDev.InTouch;
using System;
using System.Collections.Generic;
using System.Text;

namespace VKSaver.Core.Services.Common
{
    public class VKSaverInTouch : InTouch
    {
        public VKSaverInTouch(bool throwExceptionOnResponseError = false, bool includeRawResponse = false)
            : base(throwExceptionOnResponseError, includeRawResponse)
        { }

        public VKSaverInTouch(int clientId, string clientSecret, bool throwExceptionOnResponseError = false, bool includeRawResponse = false)
            : base(clientId, clientSecret, throwExceptionOnResponseError, includeRawResponse)
        { }
    }
}
