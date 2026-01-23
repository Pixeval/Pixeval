namespace Tabalonia.Events;


public class CloseLastTabEventArgs(Window? window) : EventArgs
{
    public Window? Window { get; } = window;
}