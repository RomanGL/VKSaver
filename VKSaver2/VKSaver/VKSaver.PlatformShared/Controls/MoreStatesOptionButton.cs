using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VKSaver.Controls
{
    public sealed class MoreStatesOptionButton : Control
    {
        public MoreStatesOptionButton()
        {
            this.DefaultStyleKey = typeof(MoreStatesOptionButton);
        }

        public string SymbolsForStates
        {
            get { return (string)GetValue(SymbolsForStatesProperty); }
            set { SetValue(SymbolsForStatesProperty, value); }
        }
        
        public static readonly DependencyProperty SymbolsForStatesProperty =
            DependencyProperty.Register("SymbolsForStates", typeof(string), 
                typeof(MoreStatesOptionButton), new PropertyMetadata(null));

        public int StateNumber
        {
            get { return (int)GetValue(StateNumberProperty); }
            set { SetValue(StateNumberProperty, value); }
        }
        
        public static readonly DependencyProperty StateNumberProperty =
            DependencyProperty.Register("StateNumber", typeof(int), 
                typeof(MoreStatesOptionButton), new PropertyMetadata(0, OnStateNumberChanged));

        private static void OnStateNumberChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ((MoreStatesOptionButton)obj).ChangeStateNumber();
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _content = GetTemplateChild("Content") as ContentPresenter;
            _symbols = SymbolsForStates?.ToCharArray();
            if (_symbols == null)
                throw new ArgumentNullException("SymbolsForStates");

            _statesCount = _symbols.Length;

            this.PointerPressed += MoreStatesOptionButton_PointerPressed;
            this.PointerReleased += MoreStatesOptionButton_PointerReleased;
            this.Tapped += MoreStatesOptionButton_Tapped;

            ChangeStateNumber();
        }

        private void MoreStatesOptionButton_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (StateNumber + 1 == _statesCount)
                StateNumber = 0;
            else
                StateNumber++;
        }

        private void MoreStatesOptionButton_PointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (StateNumber == 0)
                VisualStateManager.GoToState(this, "Normal", true);
            else
                VisualStateManager.GoToState(this, "Checked", true);
        }

        private void MoreStatesOptionButton_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (StateNumber == 0)
                VisualStateManager.GoToState(this, "Pressed", true);
            else
                VisualStateManager.GoToState(this, "CheckedPressed", true);
        }

        private void ChangeStateNumber()
        {
            if (_symbols == null)
                return;

            if (StateNumber == 0)
            {
                if (IsEnabled)
                    VisualStateManager.GoToState(this, "Normal", true);
                else
                    VisualStateManager.GoToState(this, "Disabled", true);
            }
            else
            {
                if (IsEnabled)
                    VisualStateManager.GoToState(this, "Checked", true);
                else
                    VisualStateManager.GoToState(this, "CheckedDisabled", true);
            }

            _content.Content = _symbols[StateNumber].ToString();
        }

        private ContentPresenter _content;
        private char[] _symbols;
        private int _statesCount;
    }
}
