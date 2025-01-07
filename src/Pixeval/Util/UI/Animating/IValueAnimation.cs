// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Numerics;
using System.Threading.Tasks;

namespace Pixeval.Util.UI.Animating;

public interface IValueAnimation<out TV> where TV : INumber<TV>
{
    TimeSpan Duration { get; }

    TimeSpan SampleRate { get; }

    TV From { get; }

    TV To { get; }

    IEasingFunction<TV>? EasingFunction { get; }

    event Action<TV> OnValueChanged;

    event EventHandler OnCompleted;

    Task StartAsync();
}