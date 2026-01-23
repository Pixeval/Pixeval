using System;
using System.Threading;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Media;
using Avalonia.Styling;

namespace FluentAvalonia.UI.Media.Animation;

/// <summary>
/// Specifies the animation to run when a user navigates forward in a logical hierarchy, 
/// like from a master list to a detail page.
/// </summary>
public class DrillInNavigationTransitionInfo : NavigationTransitionInfo
{
    /// <summary>
    /// Gets or sets whether the animation should drill in (false) or drill out (true)
    /// </summary>
    public bool IsReversed { get; set; } = false; //Zoom out if true

    //Zoom & Fade
    public async override void RunAnimation(Animatable ctrl, CancellationToken cancellationToken)
    {
        var animation = new Avalonia.Animation.Animation
        {
            Easing = new SplineEasing(0.1, 0.9, 0.2, 1.0),
            Children =
            {
                new KeyFrame
                {
                    Setters =
                    {
                        new Setter(Visual.OpacityProperty, 0.0),
                        new Setter(ScaleTransform.ScaleXProperty, IsReversed ? 1.5 : 0.0),
                        new Setter(ScaleTransform.ScaleYProperty, IsReversed ? 1.5 : 0.0)
                    },
                    Cue = new Cue(0d)
                },
                new KeyFrame
                {
                    Setters =
                    {
                        new Setter(Visual.OpacityProperty, 1.0),
                        new Setter(ScaleTransform.ScaleXProperty, IsReversed ? 1.0 : 1.0),
                        new Setter(ScaleTransform.ScaleYProperty, IsReversed ? 1.0 : 1.0)
                    },
                    Cue = new Cue(1d)
                }
            },
            Duration = TimeSpan.FromSeconds(0.67),
            FillMode = FillMode.Forward
        };

        await animation.RunAsync(ctrl, cancellationToken);

        (ctrl as Visual)?.Opacity = 1;
    }
}

