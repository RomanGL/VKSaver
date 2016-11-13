using IF.Lastfm.Core.Api;
using Microsoft.Practices.Unity;

namespace VKSaver.Core
{
    internal static class InstanceFactories
    {
        public static LastfmClient ResolveLastfmClient(IUnityContainer container)
        {
            var lastAuth = container.Resolve<ILastAuth>();
            return new LastfmClient((LastAuth)lastAuth);
        }
    }
}
