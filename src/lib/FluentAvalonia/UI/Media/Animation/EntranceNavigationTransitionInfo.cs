using System;
using System.Threading;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Media;
using Avalonia.Styling;

namespace FluentAvalonia.UI.Media.Animation;

/// <summary>
/// Specifies the animation to run when content appears on a Page.
/// </summary>
public class EntranceNavigationTransitionInfo : NavigationTransitionInfo
{
    /// <summary>
    /// Gets or sets the Horizontal Offset used when animating
    /// </summary>
    public double FromHorizontalOffset { get; set; } = 0;

    /// <summary>
    /// Gets or sets the Vertical Offset used when animating
    /// </summary>
    public double FromVerticalOffset { get; set; } = 100;

    //SlideUp and FadeIn
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
                        new Setter(TranslateTransform.XProperty,FromHorizontalOffset),
                        new Setter(TranslateTransform.YProperty, FromVerticalOffset)
                    },
                    Cue = new Cue(0d)
                },
                new KeyFrame
                {
                    Setters =
                    {
                        new Setter(Visual.OpacityProperty, 1d),
                        new Setter(TranslateTransform.XProperty,0.0),
                        new Setter(TranslateTransform.YProperty, 0.0)
                    },
                    Cue = new Cue(1d)
                }
            },
            Duration = TimeSpan.FromSeconds(0.5),
            FillMode = FillMode.Forward
        };

        await animation.RunAsync(ctrl, cancellationToken);

        (ctrl as Visual)?.Opacity = 1;
    }
}

