using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;
using ModernDev.InTouch;

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

        public object Thumbs
        {
            get { return (object)GetValue(ThumbsProperty); }
            set { SetValue(ThumbsProperty, value); }
        }

        public static readonly DependencyProperty ThumbsProperty = DependencyProperty.Register(
            "Thumbs", typeof(object), typeof(ImagePlaceholder), new PropertyMetadata(default(object), OnThumbsChanged));

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

        private static void OnThumbsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            (obj as ImagePlaceholder)?.TryLoadThumbs();
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
                else if (Source is Uri)
                    imgSource = new BitmapImage((Uri)Source);
                else
                    return;

                _brush = new ImageBrush
                {
                    ImageSource = imgSource,
                    Stretch = Stretch.UniformToFill,
                    AlignmentX = AlignmentX.Center,
                    AlignmentY = AlignmentY.Center
                };

                _rect = new Rectangle();
                _rect.Fill = _brush;

                _brush.ImageOpened += Image_ImageOpened;
                _brush.ImageFailed += Image_ImageFailed;

                this.Children.Add(_rect);
            }
            catch (Exception) { }
        }

        private void Image_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            _brush.ImageOpened -= Image_ImageOpened;
            _brush.ImageFailed -= Image_ImageFailed;

            this.Children.Remove(_rect);
            _rect = null;
            _brush = null;            
        }

        private void Image_ImageOpened(object sender, RoutedEventArgs e)
        {
            _brush.ImageOpened -= Image_ImageOpened;
            _brush.ImageFailed -= Image_ImageFailed;

            this.Children.Remove(_textImage);
            _textImage = null;
        }

        private void TryLoadThumbs()
        {
            _loaded = true;
            this.Children.Clear();

            _textImage = new TextImage() { Text = this.Text };
            this.Children.Add(_textImage);

            try
            {
                List<Thumb> thumbs = null;
                if (Thumbs is List<Thumb>)
                    thumbs = (List<Thumb>)Thumbs;

                _grid = new Grid();
                _grid.HorizontalAlignment = HorizontalAlignment.Stretch;
                _grid.VerticalAlignment = VerticalAlignment.Stretch;

                if (thumbs.Count == 1)
                {
                    var rect = GetImageRectangle(thumbs[0].Photo300);
                    _grid.Children.Add(rect);
                }
                else if (thumbs.Count >= 2)
                {
                    _grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
                    _grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
                    _grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
                    _grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });

                    if (thumbs.Count == 2)
                    {
                        var rect1 = GetImageRectangle(thumbs[0].Photo300);
                        var rect2 = GetImageRectangle(thumbs[1].Photo300);

                        SetRectsGridPosition(rect1, rect2);

                        _grid.Children.Add(rect1);
                        _grid.Children.Add(rect2);
                    }
                    else if (thumbs.Count == 3)
                    {
                        var rect1 = GetImageRectangle(thumbs[0].Photo300);
                        var rect2 = GetImageRectangle(thumbs[1].Photo300);
                        var rect3 = GetImageRectangle(thumbs[2].Photo300);

                        SetRectsGridPosition(rect1, rect2, rect3);

                        _grid.Children.Add(rect1);
                        _grid.Children.Add(rect2);
                        _grid.Children.Add(rect3);
                    }
                    else
                    {
                        var rect1 = GetImageRectangle(thumbs[0].Photo300);
                        var rect2 = GetImageRectangle(thumbs[1].Photo300);
                        var rect3 = GetImageRectangle(thumbs[2].Photo300);
                        var rect4 = GetImageRectangle(thumbs[3].Photo300);

                        SetRectsGridPosition(rect1, rect2, rect3, rect4);

                        _grid.Children.Add(rect1);
                        _grid.Children.Add(rect2);
                        _grid.Children.Add(rect3);
                        _grid.Children.Add(rect4);
                    }
                }
                else
                    return;

                this.Children.Add(_grid);
            }
            catch (Exception) { }
        }

        private Rectangle GetImageRectangle(string url)
        {
            var brush = new ImageBrush
            {
                ImageSource = new BitmapImage(new Uri(url)),
                Stretch = Stretch.UniformToFill,
                AlignmentX = AlignmentX.Center,
                AlignmentY = AlignmentY.Center
            };

            var rect = new Rectangle();
            rect.HorizontalAlignment = HorizontalAlignment.Stretch;
            rect.VerticalAlignment = VerticalAlignment.Stretch;

            rect.Fill = brush;

            brush.ImageOpened += (s, e) =>
            {
                _imagesCounter--;
                CheckThumbsImages();
            };
            brush.ImageFailed += (s, e) =>
            {
                _imagesCounter--;
                _grid.Children.Remove(rect);
                CheckThumbsImages();
            };
            
            _imagesCounter++;
            return rect;
        }

        private void CheckThumbsImages()
        {
            if (_imagesCounter <= 0)
            {
                if (_grid.Children.Count > 0)
                {
                    this.Children.Remove(_textImage);
                    _textImage = null;
                }
                else
                    this.Children.Remove(_grid);
            }
        }

        private void SetRectsGridPosition(params Rectangle[] rects)
        {
            _grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            _grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            _grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            _grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            if (rects.Length < 2)
                throw new ArgumentOutOfRangeException("The number of rectangles must greater than 2.");

            double halfWidth = this.Width / 2;
            double halfHeight = this.Height / 2;

            if (rects.Length == 2)
            {
                var rect1 = rects[0];
                var rect2 = rects[1];

                int type = _random.Next(0, 2);
                switch (type)
                {
                    case 0:
                        Grid.SetColumnSpan(rect1, 2);
                        Grid.SetRow(rect1, 0);
                        rect1.Width = this.Width;
                        rect1.Height = halfHeight;

                        Grid.SetColumnSpan(rect2, 2);
                        Grid.SetRow(rect2, 1);
                        rect2.Width = this.Width;
                        rect2.Height = halfHeight;
                        break;
                    case 1:
                        Grid.SetColumn(rect1, 0);
                        Grid.SetRowSpan(rect1, 2);
                        rect1.Width = halfWidth;
                        rect1.Height = this.Height;

                        Grid.SetColumn(rect2, 1);
                        Grid.SetRowSpan(rect2, 2);
                        rect2.Width = halfWidth;
                        rect2.Height = this.Height;
                        break;
                }
            }
            else if (rects.Length == 3)
            {
                var rect1 = rects[0];
                var rect2 = rects[1];
                var rect3 = rects[2];

                int type = _random.Next(0, 4);
                switch (type)
                {
                    case 0:
                        Grid.SetColumn(rect1, 0);
                        Grid.SetRowSpan(rect1, 2);
                        rect1.Width = halfWidth;
                        rect1.Height = this.Height;

                        Grid.SetColumn(rect2, 1);
                        Grid.SetRow(rect2, 0);
                        rect2.Width = halfWidth;
                        rect2.Height = halfHeight;

                        Grid.SetColumn(rect3, 1);
                        Grid.SetRow(rect3, 1);
                        rect3.Width = halfWidth;
                        rect3.Height = halfHeight;
                        break;
                    case 1:
                        Grid.SetColumn(rect1, 0);
                        Grid.SetRow(rect1, 0);
                        rect1.Width = halfWidth;
                        rect1.Height = halfHeight;

                        Grid.SetColumn(rect2, 0);
                        Grid.SetRow(rect2, 1);
                        rect2.Width = halfWidth;
                        rect2.Height = halfHeight;

                        Grid.SetColumn(rect3, 1);
                        Grid.SetRowSpan(rect3, 2);
                        rect3.Width = halfWidth;
                        rect3.Height = this.Height;
                        break;
                    case 2:
                        Grid.SetColumn(rect1, 0);
                        Grid.SetRow(rect1, 0);
                        rect1.Width = halfWidth;
                        rect1.Height = halfHeight;

                        Grid.SetColumnSpan(rect2, 2);
                        Grid.SetRow(rect2, 1);
                        rect2.Width = this.Width;
                        rect2.Height = halfHeight;

                        Grid.SetColumn(rect3, 1);
                        Grid.SetRow(rect3, 0);
                        rect3.Width = halfWidth;
                        rect3.Height = halfHeight;
                        break;
                    case 3:
                        Grid.SetColumnSpan(rect1, 2);
                        Grid.SetRow(rect1, 0);
                        rect1.Width = this.Width;
                        rect1.Height = halfHeight;

                        Grid.SetColumn(rect2, 0);
                        Grid.SetRow(rect2, 1);
                        rect2.Width = halfWidth;
                        rect2.Height = halfHeight;

                        Grid.SetColumn(rect3, 1);
                        Grid.SetRow(rect3, 1);
                        rect3.Width = halfWidth;
                        rect3.Height = halfHeight;
                        break;
                }
            }
            else
            {
                var rect1 = rects[0];
                var rect2 = rects[1];
                var rect3 = rects[2];
                var rect4 = rects[3];

                Grid.SetColumn(rect1, 0);
                Grid.SetRow(rect1, 0);
                rect1.Width = halfWidth;
                rect1.Height = halfHeight;

                Grid.SetColumn(rect2, 0);
                Grid.SetRow(rect2, 1);
                rect2.Width = halfWidth;
                rect2.Height = halfHeight;

                Grid.SetColumn(rect3, 1);
                Grid.SetRow(rect3, 0);
                rect3.Width = halfWidth;
                rect3.Height = halfHeight;

                Grid.SetColumn(rect4, 1);
                Grid.SetRow(rect4, 1);
                rect4.Width = halfWidth;
                rect4.Height = halfHeight;
            }
        }

        private int _imagesCounter;
        private TextImage _textImage;
        private ImageBrush _brush;
        private Rectangle _rect;
        private Grid _grid;
        private bool _loaded;

        private static readonly Random _random = new Random(Environment.TickCount);
    }
}
