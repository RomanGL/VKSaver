using System;
using System.Collections.Generic;
using System.Text;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace VKSaver.Controls
{
    public class TextImage : Control
    {
        public TextImage()
        {
            this.DefaultStyleKey = typeof(TextImage);            
            this.Loaded += TextImage_Loaded;
            this.Unloaded += TextImage_Unloaded;
        }

        public string Text
        {
            get { return (string)GetValue(Line1Property); }
            set { SetValue(Line1Property, value); }
        }
        
        public static readonly DependencyProperty Line1Property =
            DependencyProperty.Register("Text", typeof(string), 
                typeof(TextImage), new PropertyMetadata(null, OnTextChanged));
        
        private static void OnTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            (obj as TextImage)?.SetNewSymbols();
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _canvas = GetTemplateChild("RootCanvas") as Canvas;
            _firstSymbol = GetTemplateChild("FirstSymbol") as TextBlock;
            _secondSymbol = GetTemplateChild("SecondSymbol") as TextBlock;

            CalculateSizes();
            SetNewSymbols();
        }

        private void CalculateSizes()
        {
            if (_canvas == null || _firstSymbol == null || _secondSymbol == null ||
                ActualHeight == 0 || ActualWidth == 0)
                return;

            _canvas.Clip = new RectangleGeometry()
            {
                Rect = new Rect(0, 0, ActualWidth, ActualHeight)
            };
            _firstSymbol.FontSize = ActualWidth * 1.7;
            _secondSymbol.FontSize = ActualWidth * 1.7;

            Canvas.SetLeft(_firstSymbol, -ActualWidth / 6);
            Canvas.SetLeft(_secondSymbol, ActualWidth / 4);
            Canvas.SetTop(_secondSymbol, ActualHeight / 10);
        }

        private void SetNewSymbols()
        {
            if (_canvas == null || _firstSymbol == null || _secondSymbol == null)
                return;

            var symbols = GetSymbolsFromString(Text);
            _firstSymbol.Text = symbols[0].ToString();
            _secondSymbol.Text = symbols[1].ToString();
        }

        private char[] GetSymbolsFromString(string text)
        {
            var symbols = new char[2];
            int symbolsSelected = 0;

            if (!String.IsNullOrWhiteSpace(text))
            {
                var words = text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (words.Length == 1)
                {
                    int wordLength = words[0].Length;

                    for (int i = 0; i < wordLength && symbolsSelected != 2; i++)
                    {
                        if (char.IsLetter(words[0][i]))
                        {
                            symbols[symbolsSelected] = char.ToUpper(words[0][i]);
                            symbolsSelected++;
                        }
                        else if (char.IsDigit(words[0][i]))
                        {
                            symbols[symbolsSelected] = words[0][i];
                            symbolsSelected++;
                        }
                    }
                }
                else if (words.Length >= 2)
                {
                    for (int i = 0; i < words.Length && symbolsSelected != 2; i++)
                    {
                        int wordLength = words[i].Length;
                        for (int j = 0; j < wordLength; j++)
                        {
                            if (char.IsLetter(words[i][j]))
                            {
                                symbols[symbolsSelected] = char.ToUpper(words[i][j]);
                                symbolsSelected++;
                                break;
                            }
                            else if (char.IsDigit(words[i][j]))
                            {
                                symbols[symbolsSelected] = words[i][j];
                                symbolsSelected++;
                                break;
                            }
                        }
                    }
                }
            }

            switch (symbolsSelected)
            {
                case 0:
                    symbols[0] = 'V';
                    symbols[1] = 'S';
                    break;
                case 1:
                    symbols[1] = 'S';
                    break;
                default:
                    break;
            }

            return symbols;
        }

        private void TextImage_Loaded(object sender, RoutedEventArgs e)
        {     
            this.SizeChanged += TextImage_SizeChanged;
            this.Loaded -= TextImage_Loaded;

            CalculateSizes();
            SetNewSymbols();
        }

        private void TextImage_Unloaded(object sender, RoutedEventArgs e)
        {
            this.SizeChanged -= TextImage_SizeChanged;
            this.Unloaded -= TextImage_Unloaded;
        }

        private void TextImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            CalculateSizes();
        }

        private Canvas _canvas;
        private TextBlock _firstSymbol;
        private TextBlock _secondSymbol;
    }
}
