// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Transformation;
using Avalonia.Rendering.Composition;
using Avalonia.Rendering.Composition.Animations;

namespace Pixeval.Controls;

public sealed class WaveProgressHeart : TemplatedControl
{
    private const double TranslateTransformMinY = -5;

    private double _translateTransformYRange;
    private bool _loaded;
    private bool _initialPositionSet;
    private Border? _borderClip;
    private Path? _pathWave;
    private static readonly TimeSpan _MinTimeSpan = TimeSpan.FromSeconds(0.1);

    public static readonly StyledProperty<double> ValueProperty = 
        AvaloniaProperty.Register<WaveProgressHeart, double>(nameof(Value), coerce: (o, d) => double.Clamp(d, 0, 1));

    public static readonly StyledProperty<TimeSpan> ValueTransitionDurationProperty = 
        AvaloniaProperty.Register<WaveProgressHeart, TimeSpan>(nameof(ValueTransitionDuration), TimeSpan.FromSeconds(0.5), coerce: (o, d) => d > _MinTimeSpan ? d : _MinTimeSpan);

    public static readonly StyledProperty<Easing> ValueTransitionEasingProperty = 
        AvaloniaProperty.Register<WaveProgressHeart, Easing>(nameof(ValueTransitionEasing), new LinearEasing());

    public static readonly StyledProperty<TimeSpan> WavePeriodProperty = 
        AvaloniaProperty.Register<WaveProgressHeart, TimeSpan>(nameof(WavePeriod), TimeSpan.FromSeconds(1), coerce: (o, d) => d > _MinTimeSpan ? d : _MinTimeSpan);

    public static readonly StyledProperty<double> WaveStrokeThicknessProperty =
        AvaloniaProperty.Register<WaveProgressHeart, double>(nameof(WaveStrokeThickness), 2, coerce: (o, d) => double.Max(d, 0));

    public static readonly StyledProperty<double> StrokeThicknessProperty =
        AvaloniaProperty.Register<WaveProgressHeart, double>(nameof(StrokeThickness), 1, coerce: (o, d) => double.Max(d, 0));

    public static readonly StyledProperty<IBrush?> WaveStrokeProperty =
        AvaloniaProperty.Register<WaveProgressHeart, IBrush?>(nameof(WaveStroke));

    public static readonly StyledProperty<IBrush?> StrokeProperty =
        AvaloniaProperty.Register<WaveProgressHeart, IBrush?>(nameof(Stroke));

    public static readonly StyledProperty<IBrush?> FillProperty =
        AvaloniaProperty.Register<WaveProgressHeart, IBrush?>(nameof(Fill));

    /// <summary>
    /// 0-1 value of the wave.
    /// </summary>
    public double Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public TimeSpan ValueTransitionDuration
    {
        get => GetValue(ValueTransitionDurationProperty);
        set => SetValue(ValueTransitionDurationProperty, value);
    }

    public Easing ValueTransitionEasing
    {
        get => GetValue(ValueTransitionEasingProperty);
        set => SetValue(ValueTransitionEasingProperty, value);
    }

    public TimeSpan WavePeriod
    {
        get => GetValue(WavePeriodProperty);
        set => SetValue(WavePeriodProperty, value);
    }

    public double WaveStrokeThickness
    {
        get => GetValue(WaveStrokeThicknessProperty);
        set => SetValue(WaveStrokeThicknessProperty, value);
    }

    public double StrokeThickness
    {
        get => GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    public IBrush? WaveStroke
    {
        get => GetValue(WaveStrokeProperty);
        set => SetValue(WaveStrokeProperty, value);
    }

    public IBrush? Stroke
    {
        get => GetValue(StrokeProperty);
        set => SetValue(StrokeProperty, value);
    }

    public IBrush? Fill
    {
        get => GetValue(FillProperty);
        set => SetValue(FillProperty, value);
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.Property == ValueProperty)
            UpdateWave(e.GetNewValue<double>());
        else if (e.Property == ValueTransitionDurationProperty || e.Property == ValueTransitionEasingProperty)
            UpdateValueTransition();
        else if (e.Property == WavePeriodProperty)
            UpdateWavePeriod(e.GetNewValue<TimeSpan>());
        else if (e.Property == BoundsProperty)
        {
            if (_borderClip is null)
                return;

            var newRange = _borderClip.Bounds.Height - TranslateTransformMinY;
            if (newRange <= 0)
                return;

            _translateTransformYRange = newRange;

            if (!_initialPositionSet)
            {
                _initialPositionSet = true;
                // 首次布局完成：先无动画直接定位，再注册 transition
                UpdateWave(Value);
                UpdateValueTransition();
            }

            UpdateWavePeriod(WavePeriod);
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _borderClip = e.NameScope.Find<Border>("PART_BorderClip");
        _pathWave = e.NameScope.Find<Path>("PART_PathWave");
    }

    private void UpdateWave(double value)
    {
        if (_pathWave is null)
            return;

        var scale = 1 - value;
        var y = (_translateTransformYRange * scale) + TranslateTransformMinY;

        var builder = TransformOperations.CreateBuilder(1);
        builder.AppendTranslate(0, y);
        _pathWave.RenderTransform = builder.Build();
    }

    private void UpdateValueTransition()
    {
        _pathWave?.Transitions =
        [
            new TransformOperationsTransition
            {
                Property = RenderTransformProperty,
                Duration = ValueTransitionDuration,
                Easing = ValueTransitionEasing
            }
        ];
    }

    private void UpdateWavePeriod(TimeSpan value)
    {
        if (_pathWave is null || ElementComposition.GetElementVisual(_pathWave) is not { } visual)
            return;

        var animation = visual.Compositor.CreateVector3DKeyFrameAnimation();
        animation.InsertKeyFrame(0f, new(0, 0, 0), new LinearEasing());
        animation.InsertKeyFrame(1f, new(-100, 0, 0), new LinearEasing());
        animation.Duration = value;
        animation.IterationBehavior = AnimationIterationBehavior.Forever;

        visual.StartAnimation(nameof(CompositionVisual.Offset), animation);
    }

    /// <inheritdoc />
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (_loaded || _borderClip is null)
            return;
        _loaded = true;

        // 定位和 transition 注册在 BoundsProperty 首次有效变化时进行，
        // 确保 _translateTransformYRange 已正确计算，Wave 直接出现在正确位置。

        // Start wave animation
        UpdateWavePeriod(WavePeriod);
    }

    /// <inheritdoc />
    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        _loaded = false;
        _initialPositionSet = false;
        
        if (_pathWave is not null && ElementComposition.GetElementVisual(_pathWave) is { } visual)
            visual.StopAnimation(nameof(CompositionVisual.Offset));
    }
}
