using System.Windows.Input;

namespace VKSaver.Core.Toolkit.Controls
{
    public interface IButtonElement
    {
        string Label { get; }
        IButtonIcon Icon { get; }
        ICommand Command { get; }
    }
}
