using Windows.ApplicationModel.Resources;

namespace VKSaver.Common
{
    public class LocalizationXamlWrapper
    {
        public LocalizationXamlWrapper()
        {
            _loader = ResourceLoader.GetForViewIndependentUse();
        }

        public string this[string resName] { get { return _loader.GetString(resName); } }

        private readonly ResourceLoader _loader;
    }
}
