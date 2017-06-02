using VKSaver.Core.Models.Common;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VKSaver.Controls
{
    [TemplateVisualState(GroupName = CommonStatesGroupName, Name = NormalStateName)]
    [TemplateVisualState(GroupName = CommonStatesGroupName, Name = MusicStateName)]
    [TemplateVisualState(GroupName = CommonStatesGroupName, Name = VideoStateName)]
    [TemplateVisualState(GroupName = CommonStatesGroupName, Name = ImageStateName)]
    public sealed partial class TransferProgressIndicator : UserControl
    {
        private const string CommonStatesGroupName = "CommonStates";
        private const string NormalStateName = "Normal";
        private const string MusicStateName = "Music";
        private const string VideoStateName = "Video";
        private const string ImageStateName = "Image";

        public TransferProgressIndicator()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Тип содержимого загрузки.
        /// </summary>
        public FileContentType ContentType
        {
            get { return (FileContentType)GetValue(ContentTypeProperty); }
            set { SetValue(ContentTypeProperty, value); }
        }

        public static readonly DependencyProperty ContentTypeProperty =
            DependencyProperty.Register("ContentType", typeof(FileContentType), typeof(TransferProgressIndicator),
                new PropertyMetadata(default(FileContentType), OnContentTypePropertyChanged));

        /// <summary>
        /// Вызывается при изменении типа содержимого загрузки.
        /// </summary>
        private static void OnContentTypePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ((TransferProgressIndicator)obj).ChangeState();
        }

        /// <summary>
        /// Процент выполнения задачи.
        /// </summary>
        public double Percentage
        {
            get { return (double)GetValue(PercentageProperty); }
            set { SetValue(PercentageProperty, value); }
        }

        public static readonly DependencyProperty PercentageProperty =
            DependencyProperty.Register("Percentage", typeof(double), typeof(TransferProgressIndicator), 
                new PropertyMetadata(default(double), OnPercentageChanged));

        /// <summary>
        /// Вызывается при изменении процентного состояния.
        /// </summary>
        private static void OnPercentageChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ((TransferProgressIndicator)obj).Progress.Value = (double)e.NewValue;
        }

        /// <summary>
        /// Определяет, требуется ли отобразить состояние паузы.
        /// </summary>
        public bool IsPaused
        {
            get { return (bool)GetValue(IsPausedProperty); }
            set { SetValue(IsPausedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsPaused.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsPausedProperty =
            DependencyProperty.Register("IsPaused", typeof(bool), typeof(TransferProgressIndicator), 
                new PropertyMetadata(default(bool), OnIsPausedChanged));

        /// <summary>
        /// Вызывается при изменении процентного состояния.
        /// </summary>
        private static void OnIsPausedChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ((TransferProgressIndicator)obj).Progress.ShowPaused = (bool)e.NewValue;
        }

        /// <summary>
        /// Изменить состояние.
        /// </summary>
        private void ChangeState()
        {
            string stateName;
            switch (ContentType)
            {
                case FileContentType.Music:
                    stateName = MusicStateName;
                    break;
                case FileContentType.Video:
                    stateName = VideoStateName;
                    break;
                case FileContentType.Image:
                    stateName = ImageStateName;
                    break;
                default:
                    stateName = NormalStateName;
                    break;
            }
            VisualStateManager.GoToState(this, stateName, true);
        }
    }
}
