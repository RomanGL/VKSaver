using Microsoft.Practices.Prism.StoreApps;
using VKSaver.Controls;
using VKSaver.Core.ViewModels.Common;
using Windows.ApplicationModel.Activation;

namespace VKSaver.Views
{
    public sealed partial class UploadFileView : VisualStateAwarePage, IFileOpenPickerContinuable
    {
        public UploadFileView()
        {
            this.InitializeComponent();
        }

        public void ContinueFileOpenPicker(FileOpenPickerContinuationEventArgs args)
        {
            var vm = DataContext as IFileOpenPickerSupport;
            vm?.ContinueFileOpenPicker(args.Files);
        }
    }
}
