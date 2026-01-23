using FluentAvalonia.UI.Media.Animation;

namespace FluentAvalonia.UI.Navigation;

/// <summary>
/// Represents options for a frame navigation, including whether it is recorded in the 
/// navigation stack and what transition animation is used.
/// </summary>
public class FrameNavigationOptions
{
    /// <summary>
    /// Gets or sets a value that indicates the animated transition associated with the navigation.
    /// </summary>
    public NavigationTransitionInfo? TransitionInfoOverride { get; set; }

    /// <summary>
    /// Gets or sets a value that indicates whether navigation is recorded in the Frame's ForwardStack or BackStack.
    /// </summary>
    public bool IsNavigationStackEnabled { get; set; }
}
