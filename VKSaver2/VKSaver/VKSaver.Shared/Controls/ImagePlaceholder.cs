using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace VKSaver.Controls
{
    public class ImagePlaceholder : Grid
    {
        public ImagePlaceholder()
        {            
            this.Loaded += ImagePlaceholder_Loaded;
        }

        public Brush BackgroundBrush
        {
            get { return (Brush)GetValue(BackgroundBrushProperty); }
            set { SetValue(BackgroundBrushProperty, value); }
        }
        
        public static readonly DependencyProperty BackgroundBrushProperty =
            DependencyProperty.Register("BackgroundBrush", typeof(Brush), 
                typeof(ImagePlaceholder), new PropertyMetadata(default(Brush), OnBackgroundBrushChanged));

        private static void OnBackgroundBrushChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var textImage = ((ImagePlaceholder)obj)._textImage;
            if (textImage != null)
                textImage.Background = (Brush)e.NewValue;
        }

        public object Source
        {
            get { return (object)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }
        
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(object), 
                typeof(ImagePlaceholder), new PropertyMetadata(default(object), OnSourceChanged));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string),
                typeof(ImagePlaceholder), new PropertyMetadata(default(string), OnTextChanged));

        private static void OnSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            (obj as ImagePlaceholder)?.TryLoadImage();
        }

        private static void OnTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var instance = obj as ImagePlaceholder;
            if (instance == null)
                return;

            if (instance._textImage != null)
                instance._textImage.Text = e.NewValue as string;
        }

        private void ImagePlaceholder_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= ImagePlaceholder_Loaded;

            if (!_loaded)
                TryLoadImage();
        }

        private void TryLoadImage()
        {
            _loaded = true;
            this.Children.Clear();

            _textImage = new TextImage() { Text = this.Text };
            this.Children.Add(_textImage);

            try
            {
                ImageSource imgSource = null;

                if (Source is string && !String.IsNullOrEmpty((string)Source))
                    imgSource = new BitmapImage(new Uri((string)Source));
                else if (Source is ImageSource)
                    imgSource = (ImageSource)Source;
                else
                    return;

                _image = new Image()
                {
                    Stretch = Stretch.UniformToFill,
                    Source = imgSource
                };

                _image.ImageOpened += Image_ImageOpened;
                _image.ImageFailed += Image_ImageFailed;

                this.Children.Add(_image);
            }
            catch (Exception) { }
        }

        private void Image_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            _image.ImageOpened -= Image_ImageOpened;
            _image.ImageFailed -= Image_ImageFailed;

            this.Children.Remove(_image);
            _image = null;
        }

        private void Image_ImageOpened(object sender, RoutedEventArgs e)
        {
            _image.ImageOpened -= Image_ImageOpened;
            _image.ImageFailed -= Image_ImageFailed;

            this.Children.Remove(_textImage);
            _textImage = null;
        }

        private TextImage _textImage;
        private Image _image;
        private bool _loaded;
    }
}
