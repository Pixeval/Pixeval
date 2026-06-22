using System.Windows.Input;

namespace Pixeval.Controls;

public sealed class TokenizingBoxToken(object? item, ICommand removeCommand)
{
    public object? Item { get; } = item;

    public ICommand RemoveCommand { get; } = removeCommand;
}
