#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/ValueAnimations.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Threading.Tasks;

namespace Pixeval.Util.UI.Animating;

public static class ValueAnimations
{
    internal static async IAsyncEnumerable<V> StartAsync<V>(IValueAnimation<V> valueAnimation) where V : INumber<V>
    {
        await Task.Yield();
        var start = valueAnimation.From;
        var sampleCount = (int)Math.Ceiling(valueAnimation.Duration.Divide(valueAnimation.SampleRate));
        var by = valueAnimation.To - valueAnimation.From;
        var delta = by / V.Parse(sampleCount.ToString(), NumberStyles.Integer, null);
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