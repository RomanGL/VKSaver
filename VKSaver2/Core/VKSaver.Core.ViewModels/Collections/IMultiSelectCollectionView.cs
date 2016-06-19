using Windows.UI.Xaml.Controls.Primitives;

namespace VKSaver.Core.ViewModels.Collections
{
    public interface IMultiSelectCollectionView
    {
        void AddControl(Selector selector);
        void RemoveControl(Selector selector);
    }
}
