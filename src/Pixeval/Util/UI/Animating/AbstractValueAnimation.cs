// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace Pixeval.Util.UI.Animating;

public abstract class AbstractValueAnimation<TValue> : IValueAnimation<TValue> where TValue : INumber<TValue>
{
    public abstract TimeSpan Duration { get; }

    public abstract TimeSpan SampleRate { get; }

    public abstract TValue From { get; }

    public abstract TValue To { get; }

    public abstract IEasingFunction<TValue>? EasingFunction { get; }

    public event Action<TValue>? OnValueChanged;

    public event EventHandler? OnCompleted;

    public Task StartAsync()
    {
        return ValueAnimations.StartAsync(this)
            .ForEachAsync(e => OnValueChanged?.Invoke(e))
            .ContinueWith(_ => OnCompleted?.Invoke(this, EventArgs.Empty));
    }
}
