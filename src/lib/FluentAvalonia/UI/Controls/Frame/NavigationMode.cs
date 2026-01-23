namespace FluentAvalonia.UI.Navigation;

/// <summary>
/// Specifies the navigation stack characteristics of a navigation.
/// </summary>
public enum NavigationMode
{
    /// <summary>
    /// Navigation is to a new instance of a page (not going forward or backward in the stack).
    /// </summary>
    New = 0,

    /// <summary>
    /// Navigation is going backward in the stack.
    /// </summary>
    Back,

    /// <summary>
    /// Navigation is going forward in the stack.
    /// </summary>
    Forward,

    /// <summary>
    /// Navigation is to the current page (perhaps with different data).
    /// </summary>
    Refresh
}
