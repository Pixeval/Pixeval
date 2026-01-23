using System.Threading;
using Avalonia;
using Avalonia.Animation;

namespace FluentAvalonia.UI.Media.Animation;

/// <summary>
/// Specifies that animations are suppressed during navigation.
/// </summary>
public class SuppressNavigationTransitionInfo : NavigationTransitionInfo
{
    public override void RunAnimation(Animatable ctrl, CancellationToken cancellationToken)
    {
        //Do nothing
        (ctrl as Visual).Opacity = 1;
    }
}

