using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.ViewManagement;
using System.Threading.Tasks;
using VKSaver.Controls.Common;
using VKSaver.Core.Services.Interfaces;
using Microsoft.Practices.ServiceLocation;

namespace VKSaver.Controls
{
    /// <summary>
    /// Представляет анимированый фон аудиопроигрывателя.
    /// </summary>
    public partial class PlayerBackground : UserControl
    {
        /// <summary>
        /// Рассчитать размеры.
        /// </summary>
        /// <param name="screenWidth">Ширина экрана приложения.</param>
        /// <param name="screenHeight">Высота экрана приложения.</param>
        private static void CalculateSizes(int screenWidth, int screenHeight)
        {
            if (screenWidth > screenHeight)
            {
                LineSize = screenHeight / 3;
                RowsCount = 4;
                ColumnsCount = 8;
            }
            else
            {
                LineSize = screenWidth / 3;
                RowsCount = 6;
                ColumnsCount = 6;
            }

            LineSize = screenWidth > screenHeight ? screenHeight / 3 : screenWidth / 3;
            HalfLineSize = LineSize / 2;
            LeftMargins = new int[ColumnsCount];
            TopMargins = new int[RowsCount];

            int tempRows = 0;
            for (int i = 0; i < RowsCount; i++)
            {
                TopMargins[i] = tempRows;
                tempRows += LineSize;
            }
            int tempColumns = -LineSize;
            for (int i = 0; i < ColumnsCount; i++)
            {
                LeftMargins[i] = tempColumns;
                tempColumns += LineSize;
            }
        }

        #region События
        /// <summary>
        /// Происходит при изменении темы проигрывателя.
        /// </summary>
        public event EventHandler<Color> ThemeChanged;
        #endregion

        #region Указатели свойств для анимаций
        private const string OpacityPropertyPath = "(UIElement.Opacity)";
        private const string ScaleXPropertyPath = "(UIElement.RenderTransform).(CompositeTransform.ScaleX)";
        private const string ScaleYPropertyPath = "(UIElement.RenderTransform).(CompositeTransform.ScaleY)";
        private const string FillProperty = "(Shape.Fill).(SolidColorBrush.Color)";

        private static int RowsCount;
        private static int ColumnsCount;

        private const double SUBSTRATE_COLOR_ANIMATION_DURATION = 4.0;
        private const int CHANGE_THEME_DELAY = 30;
        private const uint MAX_ALBUMS_IMAGES_COUNT = 30;

        private const int ShapeTypesCount = 9;
        private const int SizeFactorsCount = 3;
        private const int SubstrateColorsCount = 6;
        private const int MagentaColorsCount = 7;
        private const int GreenColorsCount = 6;
        private const int OrangeColorsCount = 7;
        private const int BlueColorsCount = 7;
        private const int BlackColorsCount = 1;
        private const int GrayColorsCount = 1;

        private readonly Random RandomFactory = new Random(Environment.TickCount);
        private readonly DispatcherTimer Timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
        private readonly DispatcherTimer ThemeTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(CHANGE_THEME_DELAY) };
        private readonly List<UIElement> ElementsOnScreen = new List<UIElement>();

        private static int[] TopMargins;
        private static int[] LeftMargins;
        private static readonly double[] SizeFactors = new double[] { 2.0, 2.5, 3.0 };
        private static int LineSize;
        private static int HalfLineSize;

        private static readonly Color[] SubstracteColors = new Color[]
        {
            new Color { A = 255, R = 234, G = 0, B = 94 },
            new Color { A = 255, R = 0, G = 150, B = 0 },
            new Color { A = 255, R = 255, G = 100, B = 0 },
            new Color { A = 255, R = 0, G = 100, B = 255 },
            new Color { A = 255, R = 0, G = 0, B = 0 },
            new Color { A = 255, R = 160, G = 160, B = 160 }
        };
        private static readonly Color[] ColorsGray = new Color[]
        {
            new Color { A = 255, R = 0, G = 0, B = 0 }
        };
        private static readonly Color[] ColorsBlack = new Color[]
        {
            new Color { A = 255, R = 130, G = 130, B = 130 }
        };
        private static readonly Color[] ColorsMagenta = new Color[]
        {
            new Color { A = 255, R = 255, G = 0, B = 255 },
            new Color { A = 255, R = 255, G = 0, B = 115 },
            new Color { A = 255, R = 255, G = 115, B = 255 },
            new Color { A = 255, R = 255, G = 0, B = 0 },
            new Color { A = 255, R = 255, G = 115, B = 130 },
            new Color { A = 255, R = 115, G = 0, B = 255 },
            new Color { A = 255, R = 115, G = 115, B = 255 }
        };
        private static readonly Color[] ColorsGreen = new Color[]
        {
            new Color { A = 255, R = 0, G = 255, B = 150 },
            new Color { A = 255, R = 0, G = 255, B = 0 },
            new Color { A = 255, R = 0, G = 255, B = 255 },
            new Color { A = 255, R = 150, G = 255, B = 0 },
            new Color { A = 255, R = 255, G = 255, B = 0 },
            new Color { A = 255, R = 0, G = 150, B = 0 }
        };
        private static readonly Color[] ColorsOrange = new Color[]
        {
            new Color { A = 255, R = 255, G = 0, B = 0 },
            new Color { A = 255, R = 255, G = 70, B = 0 },
            new Color { A = 255, R = 255, G = 150, B = 0 },
            new Color { A = 255, R = 255, G = 255, B = 0 },
            new Color { A = 255, R = 255, G = 0, B = 80 },
            new Color { A = 255, R = 255, G = 0, B = 150 },
            new Color { A = 255, R = 255, G = 100, B = 100 }
        };
        private static readonly Color[] ColorsBlue = new Color[]
        {
            new Color { A = 255, R = 0, G = 0, B = 255 },
            new Color { A = 255, R = 0, G = 100, B = 255 },
            new Color { A = 255, R = 0, G = 180, B = 255 },
            new Color { A = 255, R = 0, G = 255, B = 255 },
            new Color { A = 255, R = 0, G = 255, B = 180 },
            new Color { A = 255, R = 0, G = 255, B = 100 },
            new Color { A = 255, R = 80, G = 0, B = 255 }
        };
        #endregion

        #region Свойства
        public PlayerTheme BackgroundTheme
        {
            get { return _backgroundTheme; }
            private set
            {
                _backgroundTheme = value;

                Color color = SubstracteColors[(byte)value - 1];
                ChangeSubstracteColor(color);
                ChangeElementsColor();

                OnThemeChanged(SubstracteColors[(byte)value - 1]);
                ThemeBrush = new SolidColorBrush(color);
            }
        }

        private PlayerShapesType Type
        {
            get { return _type; }
            set
            {
                _type = value;

                if (DefaultShapesType == PlayerShapesType.None)
                    HideAllElements();
            }
        }
        private bool IsHiding { get; set; }

        private bool IsFirstRoot = true;
        #endregion

        #region Приватные поля        
        private int ElementsOnScreenCount = 0;
        private PlayerTheme _backgroundTheme;
        private PlayerShapesType _type;
        private IImagesCacheService _imagesCacheService;
        #endregion

        #region Свойства зависимостей

        /// <summary>
        /// Цвет текущей темы.
        /// </summary>
        public SolidColorBrush ThemeBrush
        {
            get { return (SolidColorBrush)GetValue(ThemeBrushProperty); }
            set { SetValue(ThemeBrushProperty, value); }
        }
        
        public static readonly DependencyProperty ThemeBrushProperty =
            DependencyProperty.Register("ThemeBrush", typeof(SolidColorBrush), 
                typeof(PlayerBackground), new PropertyMetadata(default(SolidColorBrush)));

        /// <summary>
        /// Имя исполнителя.
        /// </summary>
        public string ArtistName
        {
            get { return (string)GetValue(ArtistNameProperty); }
            set { SetValue(ArtistNameProperty, value); }
        }

        public static readonly DependencyProperty ArtistNameProperty =
            DependencyProperty.Register("ArtistName", typeof(string), 
                typeof(PlayerBackground), new PropertyMetadata(default(string)));

        #region ArtistImage DependencyProperty
        /// <summary>
        /// Изображение исполнителя.
        /// </summary>
        public object ArtistImage
        {
            get { return (object)GetValue(ArtistImageProperty); }
            set { SetValue(ArtistImageProperty, value); }
        }

        public static readonly DependencyProperty ArtistImageProperty =
            DependencyProperty.Register("ArtistImage", typeof(object), 
                typeof(PlayerBackground), new PropertyMetadata(default(object), OnArtistImageChanged));

        /// <summary>
        /// Вызывается при изменении изображения исполнителя.
        /// </summary>
        private static void OnArtistImageChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var control = (PlayerBackground)obj;

            ImageSource newSource = e.NewValue as ImageSource;
            if (newSource == null && e.NewValue is string)
            {
                newSource = new BitmapImage(new Uri(e.NewValue.ToString()));
            }

            control.OldImage.Fill = control.NewImage.Fill;
            control.NewImage.Fill = new ImageBrush
            {
                ImageSource = newSource,
                AlignmentX = AlignmentX.Center,
                AlignmentY = AlignmentY.Center,
                Stretch = Stretch.UniformToFill
            };

            if (e.OldValue != null)
                control.AnimateOut.Begin();
            if (e.NewValue != null)
                control.AnimateIn.Begin();
            control.NextTheme();
        }
        #endregion

        #region DefaultTheme DependencyProperty

        public PlayerTheme DefaultTheme
        {
            get { return (PlayerTheme)GetValue(DefaultThemeProperty); }
            set { SetValue(DefaultThemeProperty, value); }
        }

        public static readonly DependencyProperty DefaultThemeProperty =
            DependencyProperty.Register("DefaultTheme", typeof(PlayerTheme),
                typeof(PlayerBackground), new PropertyMetadata(PlayerTheme.None, OnDefaultThemeChanged));

        private static void OnDefaultThemeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var control = (PlayerBackground)obj;
            control.NextTheme();
        }

        #endregion

        #region DefaultShapeType DependencyProperty
        public PlayerShapesType DefaultShapesType
        {
            get { return (PlayerShapesType)GetValue(DefaultShapesTypeProperty); }
            set { SetValue(DefaultShapesTypeProperty, value); }
        }

        public static readonly DependencyProperty DefaultShapesTypeProperty =
            DependencyProperty.Register("DefaultShapesType", typeof(PlayerShapesType),
                typeof(PlayerBackground), new PropertyMetadata(PlayerShapesType.None));

        #endregion

        #region NoAlbums DependencyProperty

        public bool NoAlbums
        {
            get { return (bool)GetValue(NoAlbumsProperty); }
            set { SetValue(NoAlbumsProperty, value); }
        }
        
        public static readonly DependencyProperty NoAlbumsProperty =
            DependencyProperty.Register("NoAlbums", typeof(bool), 
                typeof(PlayerBackground), new PropertyMetadata(false));

        #endregion

        #region ChangeColorDuration DependencyProperty

        public double ChangeColorDuration
        {
            get { return (double)GetValue(ChangeColorDurationProperty); }
            set { SetValue(ChangeColorDurationProperty, value); }
        }
        
        public static readonly DependencyProperty ChangeColorDurationProperty =
            DependencyProperty.Register("ChangeColorDuration", typeof(double), 
                typeof(PlayerBackground), new PropertyMetadata(SUBSTRATE_COLOR_ANIMATION_DURATION));

        #endregion

        #endregion

        /// <summary>
        /// Вызывается при срабатывании таймера.
        /// </summary>
        private void Timer_Tick(object sender, object e)
        {
            if (ElementsOnScreenCount > 6)
                return;

            bool isInvertedY = RandomFactory.Next(0, 2) != 0;
            bool isInvertedX = RandomFactory.Next(0, 2) != 0;

            Color fillColor = Colors.White;

            switch (BackgroundTheme)
            {
                case PlayerTheme.Magenta:
                    fillColor = Colors.Red;
                    break;
                case PlayerTheme.Green:
                    fillColor = new Color { A = 255, R = 150, G = 255, B = 0 };
                    break;
                case PlayerTheme.Orange:
                    fillColor = new Color { A = 255, R = 255, G = 255, B = 0 };
                    break;
                case PlayerTheme.Blue:
                    fillColor = new Color { A = 255, R = 0, G = 255, B = 150 };
                    break;
                case PlayerTheme.Black:
                    fillColor = ColorsBlack[0];
                    break;
                case PlayerTheme.Gray:
                    fillColor = ColorsGray[0];
                    break;
            }


            UIElement element = null;
            int verticalMargin = 0;
            int horizontalMargin = 0;

            switch (Type)
            {
                case PlayerShapesType.HorizontalTriangles:
                    verticalMargin = isInvertedY ?
                        TopMargins[RandomFactory.Next(0, RowsCount - 1)] :
                        TopMargins[RandomFactory.Next(1, RowsCount)];
                    horizontalMargin = isInvertedX ?
                        LeftMargins[RandomFactory.Next(2, ColumnsCount)] :
                        LeftMargins[RandomFactory.Next(0, ColumnsCount - 2)];
                    element = CreateTriangle(horizontalMargin, verticalMargin, fillColor, false);
                    break;
                case PlayerShapesType.HorizontalTrapezes:
                    verticalMargin = isInvertedY ?
                        TopMargins[RandomFactory.Next(0, RowsCount - 1)] :
                        TopMargins[RandomFactory.Next(1, RowsCount)];
                    horizontalMargin = isInvertedX ?
                        LeftMargins[RandomFactory.Next(2, ColumnsCount)] :
                        LeftMargins[RandomFactory.Next(0, ColumnsCount - 2)];
                    element = CreateTrapeze(horizontalMargin, verticalMargin, fillColor, false);
                    break;
                case PlayerShapesType.VerticalTrapezes:
                    verticalMargin = isInvertedY ?
                        TopMargins[RandomFactory.Next(1, RowsCount)] :
                        TopMargins[RandomFactory.Next(0, RowsCount - 1)];
                    horizontalMargin = isInvertedX ?
                        LeftMargins[RandomFactory.Next(2, ColumnsCount - 1)] :
                        LeftMargins[RandomFactory.Next(1, ColumnsCount - 2)];
                    element = CreateTrapeze(horizontalMargin, verticalMargin, fillColor, true);
                    break;
                case PlayerShapesType.VerticalTriangles:
                    verticalMargin = TopMargins[RandomFactory.Next(0, RowsCount)];
                    horizontalMargin = isInvertedX ?
                        LeftMargins[RandomFactory.Next(2, ColumnsCount - 1)] :
                        LeftMargins[RandomFactory.Next(1, ColumnsCount - 2)];
                    element = CreateTriangle(horizontalMargin, verticalMargin, fillColor, true);
                    break;
                case PlayerShapesType.LongRhombuses:
                    verticalMargin = TopMargins[RandomFactory.Next(0, RowsCount)];
                    horizontalMargin = LeftMargins[RandomFactory.Next(1, ColumnsCount - 1)];
                    element = CreateRhombus(horizontalMargin, verticalMargin, fillColor, true);
                    break;
                case PlayerShapesType.SquareRhombuses:
                    verticalMargin = TopMargins[RandomFactory.Next(0, RowsCount)];
                    horizontalMargin = LeftMargins[RandomFactory.Next(1, ColumnsCount - 1)];
                    element = CreateRhombus(horizontalMargin, verticalMargin, fillColor, false);
                    break;
                case PlayerShapesType.HorizontalParallelograms:
                    verticalMargin = isInvertedY ?
                        TopMargins[RandomFactory.Next(0, RowsCount - 1)] :
                        TopMargins[RandomFactory.Next(1, RowsCount)];
                    horizontalMargin = isInvertedX ?
                        LeftMargins[RandomFactory.Next(2, ColumnsCount)] :
                        LeftMargins[RandomFactory.Next(0, ColumnsCount - 2)];
                    element = CreateParallelogram(horizontalMargin, verticalMargin, fillColor, false);
                    break;
                case PlayerShapesType.VerticalParallelograms:
                    verticalMargin = isInvertedY ?
                        TopMargins[RandomFactory.Next(1, RowsCount)] :
                        TopMargins[RandomFactory.Next(0, RowsCount - 1)];
                    horizontalMargin = isInvertedX ?
                        LeftMargins[RandomFactory.Next(2, ColumnsCount - 1)] :
                        LeftMargins[RandomFactory.Next(1, ColumnsCount - 2)];
                    element = CreateParallelogram(horizontalMargin, verticalMargin, fillColor, true);
                    break;
            }

            if (element == null)
                return;

            int duration = RandomFactory.Next(6, 11);
            double scaleFactor = SizeFactors[RandomFactory.Next(0, SizeFactorsCount)];

            var fadeInStoryboard = ComposeFadeInOutStoryboard(element, duration, true);
            var fadeOutStoryboard = ComposeFadeInOutStoryboard(element, duration, false);
            var resizeStoryboard = ComposeResizeStoryboard(element, scaleFactor, isInvertedX, isInvertedY, duration * 2);

            resizeStoryboard.Completed += Storyboard_Completed;
            fadeInStoryboard.Completed += (s, args) =>
            {
                if (IsHiding)
                    return;
                fadeOutStoryboard.Begin();
            };

            if (IsFirstRoot)
                RootPanel.Children.Add(element);
            else
                SubRootPanel.Children.Add(element);

            ElementsOnScreen.Add(element);
            ElementsOnScreenCount++;

            fadeInStoryboard.Begin();
            resizeStoryboard.Begin();
        }

        /// <summary>
        /// Вызывается при завершении раскадровки.
        /// </summary>
        private void Storyboard_Completed(object sender, object e)
        {
            var obj = sender as Storyboard;
            obj.Completed -= Storyboard_Completed;
            if (obj.Children.Count == 0)
                return;

            var target = StoryboardServices.GetTarget(obj.Children[0]) as UIElement;
            target.Visibility = Visibility.Collapsed;

            if (IsFirstRoot)
                RootPanel.Children.Remove(target);
            else
                SubRootPanel.Children.Remove(target);
            ElementsOnScreen.Remove(target);
            ElementsOnScreenCount--;
        }

        /// <summary>
        /// Запускается отсчет времени для обновления экрана.
        /// </summary>
        public void Start()
        {
            NextTheme();

            Timer.Tick += Timer_Tick;
            ThemeTimer.Tick += ThemeTimer_Tick;
            Timer_Tick(Timer, null);
            Timer.Start();
            ThemeTimer.Start();
        }

        /// <summary>
        /// Останавливает отсчет времени для обновления экрана.
        /// </summary>
        public void Stop()
        {
            Timer.Tick -= Timer_Tick;
            ThemeTimer.Tick -= ThemeTimer_Tick;
            Timer.Stop();
            ThemeTimer.Stop();
        }

        /// <summary>
        /// Сменяет тему проигрывателя.
        /// </summary>
        public void NextTheme()
        {
            ThemeTimer.Stop();
            albumsGridCounter = 1;
            CancelAlbumsGridState();

            if (DefaultTheme == PlayerTheme.None)
            {
                PlayerTheme theme = BackgroundTheme;
                while (theme == BackgroundTheme)
                    theme = (PlayerTheme)RandomFactory.Next(1, SubstrateColorsCount + 1);

                BackgroundTheme = theme;
            }
            else
                BackgroundTheme = DefaultTheme;

            if (DefaultShapesType == PlayerShapesType.None)
            {
                PlayerShapesType type = Type;
                while (type == Type)
                    type = (PlayerShapesType)RandomFactory.Next(1, ShapeTypesCount);

                Type = type;
            }
            else
                Type = DefaultShapesType;      

            ThemeTimer.Start();
        }

        #region Обработчики событий
        /// <summary>
        /// Вызывает событие изменения темы.
        /// </summary>
        /// <param name="newColor">Новый акцентный цвет темы.</param>
        private void OnThemeChanged(Color newColor)
        {
            if (ThemeChanged != null)
                ThemeChanged(this, newColor);
        }

        private int albumsGridCounter = 1;

        /// <summary>
        /// Вызывается при срабатывании таймера смены темы.
        /// </summary>
        private void ThemeTimer_Tick(object sender, object e)
        {
            if (DefaultTheme == PlayerTheme.None)
            {
                PlayerTheme theme = BackgroundTheme;
                while (theme == BackgroundTheme)
                    theme = (PlayerTheme)RandomFactory.Next(1, SubstrateColorsCount + 1);

                BackgroundTheme = theme;
                ChangeElementsColor();                
            }

            if (!NoAlbums)
            {
                if (--albumsGridCounter == 0)
                {
                    albumsGridCounter = 2;
                    GoToAlbumsGridState();
                }
                else
                    CancelAlbumsGridState();
            }
        }

        private void ChangeElementsColor()
        {
            Color fillColor = Colors.White;
            switch (BackgroundTheme)
            {
                case PlayerTheme.Magenta:
                    fillColor = Colors.Red;
                    break;
                case PlayerTheme.Green:
                    fillColor = new Color { A = 255, R = 150, G = 255, B = 0 };
                    break;
                case PlayerTheme.Orange:
                    fillColor = new Color { A = 255, R = 255, G = 255, B = 0 };
                    break;
                case PlayerTheme.Blue:
                    fillColor = new Color { A = 255, R = 0, G = 255, B = 150 };
                    break;
                case PlayerTheme.Black:
                    fillColor = ColorsBlack[0];
                    break;
                case PlayerTheme.Gray:
                    fillColor = ColorsGray[0];
                    break;
            }

            var storyboard = new Storyboard();
            for (int i = 0; i < ElementsOnScreen.Count; i++)
            {
                var timeline = CreateChangeColorTimeline(ElementsOnScreen[i], 
                    fillColor, ChangeColorDuration);
                storyboard.Children.Add(timeline);
            }
            storyboard.Begin();
        }

        /// <summary>
        /// Перейти к состоянию сетки альбомов.
        /// </summary>
        private async void GoToAlbumsGridState()
        {
            AlbumsPanel.Children.Clear();

            int lineSize = LineSize;
            int halfLineSize = HalfLineSize;
            int currentLeft = -lineSize / 2;
            int currentTop = -lineSize / 2;

            IList<string> images = await _imagesCacheService.GetCachedAlbumsImages(MAX_ALBUMS_IMAGES_COUNT);

            if (images == null) images = new List<string>();

            if (images.Count <= 4)
            {
                for (int i = 1; i <= 4; i++)
                    images.Add(String.Format("ms-appx:///Assets/Images/PlayerLogo{0}.png", i));
            }

            var storyboard = new Storyboard();

            while (currentTop < Window.Current.Bounds.Height)
            {
                while (currentLeft < Window.Current.Bounds.Width)
                {
                    int mode = RandomFactory.Next(1, 3);

                    if (mode == 1) // Выставляем 1 большой.
                    {
                        var img = GetAlbumImage(lineSize, lineSize, images);
                        var timelines = GetAlbumShowTransitions(img);

                        for (int i = 0; i < timelines.Length; i++)
                            storyboard.Children.Add(timelines[i]);

                        Canvas.SetLeft(img, currentLeft);
                        Canvas.SetTop(img, currentTop);

                        AlbumsPanel.Children.Add(img);
                    }
                    else if (mode == 2) // Выставляем 4 маленьких.
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            var img = GetAlbumImage(halfLineSize, halfLineSize, images);
                            var timelines = GetAlbumShowTransitions(img);

                            for (int j = 0; j < timelines.Length; j++)
                                storyboard.Children.Add(timelines[j]);

                            if (i == 0)
                            {
                                Canvas.SetTop(img, currentTop);
                                Canvas.SetLeft(img, currentLeft);
                            }
                            else if (i == 1)
                            {
                                Canvas.SetTop(img, currentTop + halfLineSize);
                                Canvas.SetLeft(img, currentLeft);
                            }
                            else if (i == 2)
                            {
                                Canvas.SetTop(img, currentTop);
                                Canvas.SetLeft(img, currentLeft + halfLineSize);
                            }
                            else if (i == 3)
                            {
                                Canvas.SetTop(img, currentTop + halfLineSize);
                                Canvas.SetLeft(img, currentLeft + halfLineSize);
                            }

                            AlbumsPanel.Children.Add(img);
                        }
                    }

                    currentLeft += lineSize;
                }

                currentTop += lineSize;
                currentLeft = -lineSize / 2;
            }

            storyboard.Begin();
        }

        /// <summary>
        /// Возвращает коллекцию анимаций появления картинки альбома.
        /// </summary>
        /// <param name="element"></param>
        private Timeline[] GetAlbumShowTransitions(UIElement element)
        {
            double duration = RandomFactory.Next(2, 15) / 10.0;
            var fadeInTimeline = CreateFadeInTimeline(element, duration, 1.0);
            var scaleXTimeline = CreateResizeWidthTimeline(element, 1.0, false, duration);
            var scaleYTimeline = CreateResizeHeightTimeline(element, 1.0, false, duration);

            ((DoubleAnimation)scaleXTimeline).EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 4 };
            ((DoubleAnimation)scaleYTimeline).EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 4 };

            return new Timeline[] { fadeInTimeline, scaleXTimeline, scaleYTimeline };
        }

        /// <summary>
        /// Возвращает картинку альбома.
        /// </summary>
        /// <param name="width">Ширина.</param>
        /// <param name="height">Высота</param>
        /// <param name="images">Список всех доступных картинок.</param>
        private Image GetAlbumImage(int width, int height, IList<string> images)
        {
            var img = new Image
            {
                Stretch = Stretch.UniformToFill,
                Width = width,
                Height = height,
                RenderTransform = new CompositeTransform(),
                RenderTransformOrigin = new Point(0.5, 0.5)
            };

            img.Source = new BitmapImage(new Uri(images[RandomFactory.Next(0, images.Count)]));

            return img;
        }

        /// <summary>
        /// Отменить режим сетки альбомов.
        /// </summary>
        private void CancelAlbumsGridState()
        {
            var storyboard = new Storyboard();

            for (int i = 0; i < AlbumsPanel.Children.Count; i++)
            {
                int k = RandomFactory.Next(1, 10);
                var timeline = CreateFadeOutTimeline(AlbumsPanel.Children[i], k / 10.0);
                storyboard.Children.Add(timeline);
            }

            storyboard.Begin();
        }

        private void CalculateObjectsSize()
        {
            double screenHeight = Window.Current.Bounds.Height;
            double screenWidth = Window.Current.Bounds.Width;

            OldImage.Height = screenHeight;
            OldImage.Width = screenWidth;

            NewImage.Height = screenHeight;
            NewImage.Width = screenWidth;

            AlbumsPanel.Height = screenHeight;
            AlbumsPanel.Width = screenWidth;

            BlackSubstrate.Height = screenHeight;
            BlackSubstrate.Width = screenWidth;

            ColorSubstrate.Height = screenHeight;
            ColorSubstrate.Width = screenWidth;

            RootPanel.Height = screenHeight;
            RootPanel.Width = screenWidth;

            SubRootPanel.Height = screenHeight;
            SubRootPanel.Width = screenWidth;
        }

        #endregion

        #region Shapes Helpers
        /// <summary>
        /// Возвращает базовый полигон.
        /// </summary>
        private static Polygon GetPolygon(Color fillColor)
        {
            var polygon = new Polygon();
            polygon.StrokeThickness = 0;
            polygon.Stretch = Stretch.Fill;
            polygon.Opacity = 0;
            polygon.Fill = new SolidColorBrush(fillColor);
            polygon.RenderTransform = new CompositeTransform();
            return polygon;
        }

        /// <summary>
        /// Возвращает новый параллелограмм.
        /// </summary>
        /// <param name="leftMargin">Отступ слева.</param>
        /// <param name="topMargin">Отступ сверху.</param>
        /// <param name="fillColor">Цвет заливки.</param>
        /// <param name="isVertical">Вертикальный ли параллелограмм.</param>
        private UIElement CreateParallelogram(int leftMargin, int topMargin, Color fillColor, bool isVertical = false)
        {
            var parallelogram = GetPolygon(fillColor);

            if (isVertical)
            {
                parallelogram.Points = new PointCollection
                {
                    new Point(150, 0), new Point(0, 75), new Point(0, 300), new Point(150, 225)
                };
                parallelogram.RenderTransformOrigin = new Point(0, 1);
                parallelogram.Height = LineSize * 3;
                parallelogram.Width = LineSize;
                Canvas.SetTop(parallelogram, topMargin - parallelogram.Height);
                Canvas.SetLeft(parallelogram, leftMargin);
            }
            else
            {
                parallelogram.Points = new PointCollection
                {
                    new Point(0, 150), new Point(225, 150), new Point(300, 0), new Point(75, 0)
                };
                parallelogram.RenderTransformOrigin = new Point(0, 1);
                parallelogram.Height = LineSize;
                parallelogram.Width = LineSize * 3;
                Canvas.SetTop(parallelogram, topMargin - parallelogram.Height);
                Canvas.SetLeft(parallelogram, leftMargin);
            }

            return parallelogram;
        }

        /// <summary>
        /// Возвращает новую трапецию.
        /// </summary>
        /// <param name="leftMargin">Отступ слева.</param>
        /// <param name="topMargin">Отступ сверху.</param>
        /// <param name="fillColor">Цвет заливки.</param>
        /// <param name="isVertical">Вертикальная ли трапеция.</param>
        private UIElement CreateTrapeze(int leftMargin, int topMargin, Color fillColor, bool isVertical = false)
        {
            var trapeze = GetPolygon(fillColor);

            if (isVertical)
            {
                trapeze.Points = new PointCollection
                {
                    new Point(0, 0), new Point(150, 75), new Point(150, 225), new Point(0, 300)
                };
                trapeze.RenderTransformOrigin = new Point(0, 0);
                trapeze.Height = LineSize * 3;
                trapeze.Width = LineSize;
                Canvas.SetTop(trapeze, topMargin);
                Canvas.SetLeft(trapeze, leftMargin);
            }
            else
            {
                trapeze.Points = new PointCollection
                {
                    new Point(0, 150), new Point(75, 0), new Point(225, 0), new Point(300, 150)
                };
                trapeze.RenderTransformOrigin = new Point(0, 1);
                trapeze.Height = LineSize;
                trapeze.Width = LineSize * 3;
                Canvas.SetTop(trapeze, topMargin - trapeze.Height);
                Canvas.SetLeft(trapeze, leftMargin);
            }
            return trapeze;
        }

        /// <summary>
        /// Возвращает новый ромб со стандартным размером.
        /// </summary>
        private UIElement CreateRhombus(int leftMargin, int topMargin, Color fillColor, bool isLong = true)
        {
            var rhombus = GetPolygon(fillColor);
            rhombus.RenderTransformOrigin = new Point(0.5, 0.5);
            rhombus.Points = new PointCollection
            {
                new Point(0, 75), new Point(75, 0), new Point(150, 75), new Point(75, 150)
            };

            if (isLong)
            {
                rhombus.Height = LineSize * 3;
                rhombus.Width = LineSize * 2;
                Canvas.SetTop(rhombus, topMargin - (LineSize * 1.5));
                Canvas.SetLeft(rhombus, leftMargin - LineSize);
            }
            else
            {
                rhombus.Height = LineSize * 2;
                rhombus.Width = rhombus.Height;
                Canvas.SetTop(rhombus, topMargin - LineSize);
                Canvas.SetLeft(rhombus, leftMargin - LineSize);
            }

            return rhombus;
        }

        /// <summary>
        /// Возвращает новый треугольник со стандартным размером.
        /// </summary>
        private UIElement CreateTriangle(int leftMargin, int verticalMargin, Color fillColor, bool isVertical = false)
        {
            var triangle = GetPolygon(fillColor);

            if (isVertical)
            {
                triangle.Points = new PointCollection
                {
                    new Point(0, 0), new Point(150, 75), new Point(0, 150)
                };
                triangle.Height = LineSize * 2;
                triangle.Width = triangle.Height;
                triangle.RenderTransformOrigin = new Point(0, 0.5);
                Canvas.SetLeft(triangle, leftMargin);
                Canvas.SetTop(triangle, verticalMargin - LineSize);
            }
            else
            {
                triangle.Points = new PointCollection
                {
                    new Point(0, 150), new Point(150, 0), new Point(300, 150)
                };
                triangle.Width = LineSize * 3;
                triangle.Height = LineSize * 1.5;
                triangle.RenderTransformOrigin = new Point(0, 1);
                Canvas.SetLeft(triangle, leftMargin);
                Canvas.SetTop(triangle, verticalMargin - triangle.Height);
            }

            return triangle;
        }
        #endregion

        #region Storyboards Helper
        /// <summary>
        /// Анимировано изменяет цвет подложки.
        /// </summary>
        /// <param name="fillColor">Новый цвет подложки.</param>
        private void ChangeSubstracteColor(Color fillColor)
        {
            var storyboard = new Storyboard();
            storyboard.Children.Add(CreateChangeColorTimeline(ColorSubstrate, 
                fillColor, ChangeColorDuration));
            storyboard.Begin();
        }

        /// <summary>
        /// Скрывает все активные элементы на экране.
        /// </summary>
        private void HideAllElements()
        {
            var storyboard = new Storyboard();

            if (IsFirstRoot)
            {
                storyboard.Children.Add(CreateFadeOutTimeline(RootPanel, 0.5));
                storyboard.Completed += (s, e) =>
                {
                    RootPanel.Children.Clear();
                    RootPanel.Opacity = 1;
                };
            }
            else
            {
                storyboard.Children.Add(CreateFadeOutTimeline(SubRootPanel, 0.5));
                storyboard.Completed += (s, e) =>
                {
                    SubRootPanel.Children.Clear();
                    SubRootPanel.Opacity = 1;
                };
            }

            storyboard.Begin();
            IsFirstRoot = IsFirstRoot ? false : true;
        }

        /// <summary>
        /// Возвращает раскадровку именения размера элемента.
        /// </summary>
        /// <param name="element">Элемент, к которому применяется раскадровка.</param>
        /// <param name="scaleFactor">Коэффициент изменения размера.</param>
        /// <param name="isInvertedX">Требуется ли инвертировать по горизонтали.</param>
        /// <param name="IsInvertedY">Требуется ли инвертировать по вертикали.</param>
        /// <param name="duration">ДЛительность раскадровки.</param>
        private static Storyboard ComposeResizeStoryboard(UIElement element, double scaleFactor,
            bool isInvertedX, bool IsInvertedY, int duration)
        {
            var storyboard = new Storyboard();

            storyboard.Children.Add(CreateResizeWidthTimeline(element, scaleFactor, isInvertedX, duration));
            storyboard.Children.Add(CreateResizeHeightTimeline(element, scaleFactor, IsInvertedY, duration));

            return storyboard;
        }

        /// <summary>
        /// Возвращает раскадровку с анимацией появления элемента.
        /// </summary>
        /// <param name="element">Элемент, к которому применяется раскадровка.</param>
        /// <param name="duration">Длительность анимации.</param>
        /// <param name="isFadeIn">Требуется ли создать анимацию появления.</param>
        private static Storyboard ComposeFadeInOutStoryboard(UIElement element, int duration, bool isFadeIn)
        {
            var storyboard = new Storyboard();
            if (isFadeIn)
                storyboard.Children.Add(CreateFadeInTimeline(element, duration));
            else
                storyboard.Children.Add(CreateFadeOutTimeline(element, duration));
            return storyboard;
        }

        /// <summary>
        /// Возвращает анимацию изменения фонового цвета элемента.
        /// </summary>
        /// <param name="element">Элемент, к которому применяется анимация.</param>
        /// <param name="fillColor">Цвет, на который требуется изменить цвет.</param>
        /// <param name="duration">ДЛительность анимации.</param>
        private static Timeline CreateChangeColorTimeline(UIElement element, Color fillColor, double duration)
        {
            var timeline = new ColorAnimation();
            timeline.Duration = TimeSpan.FromSeconds(duration);
            timeline.To = fillColor;

            StoryboardServices.SetTarget(timeline, element);
            Storyboard.SetTargetProperty(timeline, FillProperty);

            return timeline;
        }

        /// <summary>
        /// Возвращает анимацию появления для указанного элемента.
        /// </summary>
        /// <param name="element">Элемент, к которому применяется анимация.</param>
        /// <param name="duration">Длительность анимации.</param>
        /// <param name="to">Степень непрозрачности.</param>
        private static Timeline CreateFadeInTimeline(UIElement element, double duration, double to = 0.3)
        {
            var timeline = new DoubleAnimation();
            timeline.Duration = TimeSpan.FromSeconds(duration);
            timeline.From = 0;
            timeline.To = to;

            StoryboardServices.SetTarget(timeline, element);
            Storyboard.SetTargetProperty(timeline, OpacityPropertyPath);

            return timeline;
        }

        /// <summary>
        /// Вовзращает анимацию исчезновения для указанного элемента.
        /// </summary>
        /// <param name="element">Элемент, к которому применяется анимация.</param>
        /// <param name="duration">Длительность анимации.</param>
        private static Timeline CreateFadeOutTimeline(UIElement element, double duration)
        {
            var timeline = new DoubleAnimation();
            timeline.Duration = TimeSpan.FromSeconds(duration);
            timeline.To = 0;

            StoryboardServices.SetTarget(timeline, element);
            Storyboard.SetTargetProperty(timeline, OpacityPropertyPath);

            return timeline;
        }

        /// <summary>
        /// Возвращает анимацию увеличения ширины для указанного элемента.
        /// </summary>
        /// <param name="element">Элемент, к которому применяется анимация.</param>
        /// <param name="scaleFactor">Значение, во сколько раз требуется изменить ширину.</param>
        /// <param name="IsInverted">Требуется ли инвертировать значение.</param>
        /// <param name="duration">Длительность анимации.</param>
        private static Timeline CreateResizeWidthTimeline(UIElement element, double scaleFactor, bool IsInverted, double duration)
        {
            var timeline = new DoubleAnimation();
            timeline.Duration = TimeSpan.FromSeconds(duration);
            timeline.From = 0;
            if (IsInverted)
                timeline.To = -scaleFactor;
            else
                timeline.To = scaleFactor;

            StoryboardServices.SetTarget(timeline, element);
            Storyboard.SetTargetProperty(timeline, ScaleXPropertyPath);

            return timeline;
        }

        /// <summary>
        /// Возвращает анимацию увеличения высоты для указанного элемента.
        /// </summary>
        /// <param name="element">Элемент, к которому применяется анимация.</param>
        /// <param name="scaleFactor">Значение, во сколько раз требуется изменить Высоту.</param>
        /// <param name="IsInverted">Требуется ли инвертировать значение.</param>
        /// <param name="duration">Длительность анимации.</param>
        private static Timeline CreateResizeHeightTimeline(UIElement element, double resizeFactor, bool IsInverted, double duration)
        {
            var timeline = new DoubleAnimation();
            timeline.Duration = TimeSpan.FromSeconds(duration);
            timeline.From = 0;
            if (IsInverted)
                timeline.To = -resizeFactor;
            else
                timeline.To = resizeFactor;

            StoryboardServices.SetTarget(timeline, element);
            Storyboard.SetTargetProperty(timeline, ScaleYPropertyPath);

            return timeline;
        }
        #endregion
        
        /// <summary>
        /// Перечисление основных состояний.
        /// </summary>
        private enum CommonStates : byte
        {
            /// <summary>
            /// Обычное состояние.
            /// </summary>
            Normal = 0,
            /// <summary>
            /// Состояние сетки альбомов.
            /// </summary>
            AlbumsGrid
        }
    }
}