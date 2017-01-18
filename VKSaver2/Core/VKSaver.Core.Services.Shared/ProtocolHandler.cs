using System;
using System.Collections.Generic;
using System.Text;
using VKSaver.Core.Services.Interfaces;

namespace VKSaver.Core.Services
{
    public sealed class ProtocolHandler : IProtocolHandler
    {
        public void ProcessProtocol(string uri)
        {
            var parts = uri.Split(new[] { "&yamp_i" }, StringSplitOptions.None);
            string protocolUri = Uri.UnescapeDataString(parts[0].Substring(8));
            var n = new Uri(protocolUri);
        }
    }
}
