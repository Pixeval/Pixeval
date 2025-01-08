// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Threading.Tasks;

namespace Pixeval.Util.UI.Animating;

public static class ValueAnimations
{
    internal static async IAsyncEnumerable<TV> StartAsync<TV>(IValueAnimation<TV> valueAnimation) where TV : INumber<TV>
    {
        await Task.Yield();
        var start = valueAnimation.From;
        var sampleCount = (int)Math.Ceiling(valueAnimation.Duration.Divide(valueAnimation.SampleRate));
        var by = valueAnimation.To - valueAnimation.From;
        var delta = by / TV.Parse(sampleCount.ToString(), NumberStyles.Integer, null);
        yield return start;
        var counter = 1;

        while (start < valueAnimation.To)
        {
            await Task.Delay(valueAnimation.SampleRate);
            start += valueAnimation.EasingFunction is { } ef
                ? by * ef.GetValue(counter++ / (double)sampleCount)
                : delta;
            yield return start;
        }
    }
}