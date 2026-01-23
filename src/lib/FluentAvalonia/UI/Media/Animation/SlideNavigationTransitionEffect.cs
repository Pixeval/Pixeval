namespace FluentAvalonia.UI.Media.Animation;

/// <summary>
/// Defines constants that describe the type of animation to play during a slide transition.
/// </summary>
public enum SlideNavigationTransitionEffect
{
    /// <summary>
    /// The exiting page leaves to the right of the panel and the entering page enters from the left.
    /// </summary>
    FromLeft,

    /// <summary>
    /// The exiting page leaves to the left of the panel and the entering page enters from the right.
    /// </summary>
    FromRight,

    /// <summary>
    /// The exiting page fades out and the entering page enters from the top.
    /// </summary>
    FromTop,

    /// <summary>
    /// The exiting page fades out and the entering page enters from the bottom.
    /// </summary>
    FromBottom
}

