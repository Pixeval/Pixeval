// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Rendering.Composition;
using Avalonia.Rendering.Composition.Animations;

namespace Pixeval.Controls;

[PseudoClasses(PcEmpty, PcFull)]
public sealed class WaveProgressHeart : TemplatedControl
{
    private const string PcEmpty = ":empty";
    private const string PcFull = ":full";
    private const double TemplateHeight = 50;
    private const double TranslateTransformMinY = -5;

    private bool _loaded;
    private Path? _pathWave;
    private readonly TranslateTransform _waveTransform = new();
    private CompositionVisual? _animatedVisual;
    private TimeSpan _runningPeriod;
    private static readonly TimeSpan _MinTimeSpan = TimeSpan.FromSeconds(0.1);

    internal bool IsWaveVisible => Value is not 0 and not 1;

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
            UpdatePresentation();
        else if (e.Property == ValueTransitionDurationProperty || e.Property == ValueTransitionEasingProperty)
            UpdateValueTransition();
        else if (e.Property == WavePeriodProperty)
            UpdateWaveAnimation();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        StopWaveAnimation();
        base.OnApplyTemplate(e);

        _pathWave = e.NameScope.Find<Path>("PART_PathWave");
        if (_pathWave is not null)
            _pathWave.RenderTransform = _waveTransform;
        UpdatePresentation();
    }

    private void UpdatePresentation()
    {
        PseudoClasses.Set(PcEmpty, Value is 0);
        PseudoClasses.Set(PcFull, Value is 1);
        // TemplateHeight matches the fixed canvas inside the template's Viewbox. Even when
        // the clipped Border is hidden/unmeasured, wave coordinates remain valid.
        _waveTransform.Y = (TemplateHeight - TranslateTransformMinY) * (1 - Value) + TranslateTransformMinY;
        UpdateWaveAnimation();
    }

    private void UpdateValueTransition()
    {
        if (!_loaded)
            return;
        Transitions =
        [
            new DoubleTransition
            {
                Property = ValueProperty,
                Duration = ValueTransitionDuration,
                Easing = ValueTransitionEasing
            }
        ];
    }

    private void UpdateWaveAnimation()
    {
        if (!_loaded || !IsWaveVisible || _pathWave is null
            || ElementComposition.GetElementVisual(_pathWave) is not { } visual)
        {
            StopWaveAnimation();
            return;
        }
        if (ReferenceEquals(_animatedVisual, visual) && _runningPeriod == WavePeriod)
            return;

        StopWaveAnimation();

        var animation = visual.Compositor.CreateVector3DKeyFrameAnimation();
        animation.InsertKeyFrame(0f, new(0, 0, 0), new LinearEasing());
        animation.InsertKeyFrame(1f, new(-100, 0, 0), new LinearEasing());
        animation.Duration = WavePeriod;
        animation.IterationBehavior = AnimationIterationBehavior.Forever;

        visual.StartAnimation(nameof(CompositionVisual.Offset), animation);
        _animatedVisual = visual;
        _runningPeriod = WavePeriod;
    }

    private void StopWaveAnimation()
    {
        if (_animatedVisual is not { } visual)
            return;
        visual.StopAnimation(nameof(CompositionVisual.Offset));
        visual.Offset = default;
        _animatedVisual = null;
    }

    private void SnapToValue()
    {
        // Removing the transition reveals the target value without replacing its binding.
        Transitions = null;
        UpdatePresentation();
        UpdateValueTransition();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        // A recycled card's new item must not animate from the previous item's state.
        SnapToValue();
    }

    /// <inheritdoc />
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (_loaded)
            return;
        _loaded = true;

        SnapToValue();
    }

    /// <inheritdoc />
    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        _loaded = false;
        Transitions = null;
        StopWaveAnimation();
    }
}
