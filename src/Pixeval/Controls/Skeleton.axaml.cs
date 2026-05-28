// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Rendering.Composition;
using Avalonia.Rendering.Composition.Animations;

namespace Pixeval.Controls;

public sealed class Skeleton : TemplatedControl
{
    private const double ShimmerWidthRatio = 0.75;
    private const double MinShimmerWidth = 120;

    private Border? _shimmer;

    public static readonly StyledProperty<TimeSpan> ShimmerPeriodProperty =
        AvaloniaProperty.Register<Skeleton, TimeSpan>(nameof(ShimmerPeriod), TimeSpan.FromSeconds(1.2), coerce: (_, value) => value > TimeSpan.Zero ? value : TimeSpan.FromSeconds(1.2));

    public TimeSpan ShimmerPeriod
    {
        get => GetValue(ShimmerPeriodProperty);
        set => SetValue(ShimmerPeriodProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _shimmer = e.NameScope.Find<Border>("PART_Shimmer");
        UpdateShimmerAnimation();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        UpdateShimmerAnimation();
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        StopShimmerAnimation();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == BoundsProperty || e.Property == ShimmerPeriodProperty || e.Property == IsVisibleProperty)
            UpdateShimmerAnimation();
    }

    private void UpdateShimmerAnimation()
    {
        if (!IsLoaded || !IsVisible || _shimmer is null || Bounds is not { Width: > 0, Height: > 0 })
        {
            StopShimmerAnimation();
            return;
        }

        var shimmerWidth = Math.Max(MinShimmerWidth, Bounds.Width * ShimmerWidthRatio);
        _shimmer.Width = shimmerWidth;
        _shimmer.Height = Math.Max(Bounds.Height, 1);

        if (ElementComposition.GetElementVisual(_shimmer) is not { } visual)
            return;

        var animation = visual.Compositor.CreateVector3DKeyFrameAnimation();
        animation.InsertKeyFrame(0f, new((float) -shimmerWidth, 0, 0), new LinearEasing());
        animation.InsertKeyFrame(1f, new((float) (Bounds.Width + shimmerWidth), 0, 0), new LinearEasing());
        animation.Duration = ShimmerPeriod;
        animation.IterationBehavior = AnimationIterationBehavior.Forever;

        visual.StartAnimation(nameof(CompositionVisual.Offset), animation);
    }

    private void StopShimmerAnimation()
    {
        if (_shimmer is not null && ElementComposition.GetElementVisual(_shimmer) is { } visual)
            visual.StopAnimation(nameof(CompositionVisual.Offset));
    }
}
