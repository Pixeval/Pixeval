// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;

namespace Pixeval.Util.UI.Animating;

public class DoubleValueAnimation(TimeSpan duration,
        TimeSpan sampleRate,
        double from,
        double to,
        IEasingFunction<double>? easingFunction)
    : AbstractValueAnimation<double>
{
    public DoubleValueAnimation(
        TimeSpan duration,
        double from,
        double to,
        IEasingFunction<double>? easingFunction = null) : this(duration, TimeSpan.FromMilliseconds(10), from, to, easingFunction)
    {
    }

    public override TimeSpan Duration { get; } = duration;

    public override TimeSpan SampleRate { get; } = sampleRate;

    public override double From { get; } = from;

    public override double To { get; } = to;

    public override IEasingFunction<double>? EasingFunction { get; } = easingFunction;
}
