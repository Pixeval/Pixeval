// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using Pixeval.Utilities;

namespace Pixeval.Util.UI.Animating;

public record DoubleExponentialEaseOut(int Exponent = 2) : IEasingFunction<double>
{
    public static readonly IEasingFunction<double> Shared = new DoubleExponentialEaseOut();

    public double GetValue(double percentage)
    {
        if (percentage is > 1 or < 0)
            return ThrowUtils.ArgumentOutOfRange<double, double>(percentage, "The percentage must between 0 and 1.");
        return Math.Abs(percentage - 1) < double.Epsilon ? 1 : 1 - Math.Pow(Exponent, -10 * percentage);
    }
}
