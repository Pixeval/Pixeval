using System.Windows.Input;

namespace Tabalonia;

public class SimpleActionCommand(Action action) : ICommand
{
    public bool CanExecute(object? parameter) => true;

    public void Execute(object? parameter) => action.Invoke();

    public event EventHandler? CanExecuteChanged;
}

public class SimpleParamActionCommand(Action<object?> action) : ICommand
{
    public bool CanExecute(object? parameter) => true;

    public void Execute(object? parameter) => action.Invoke(parameter);

    public event EventHandler? CanExecuteChanged;
}
