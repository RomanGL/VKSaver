using System.Windows.Input;

namespace VKSaver.Core.Toolkit.Controls
{
    public class ButtonElement : IButtonElement
    {
        public ICommand Command { get; set; }

        public IButtonIcon Icon { get; set; }

        public string Label { get; set; }
    }
}
