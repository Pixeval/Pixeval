using System.Threading;
using Avalonia;
using Avalonia.Animation;

namespace FluentAvalonia.UI.Media.Animation;

//NOTE: These are custom implementations of these classes as the source is still closed...

/// <summary>
/// Provides parameter info for the Frame.Navigate method. Controls how the transition animation 
/// runs during the navigation action.
/// </summary>
public abstract class NavigationTransitionInfo : AvaloniaObject
{
    /// <summary>
    /// Executes a predefined animation on the desired object
    /// </summary>
    /// <param name="ctrl">The object to animate</param>
    /// <param name="cancellationToken"></param>
    public abstract void RunAnimation(Animatable ctrl, CancellationToken cancellationToken);
}
