#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/DoubleExponentialEaseOut.cs
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

namespace Pixeval.Util.UI.Animating;

public record DoubleExponentialEaseOut(int Exponent = 2) : IEasingFunction<double>
{
    public static readonly IEasingFunction<double> Shared = new DoubleExponentialEaseOut();

    public double GetValue(double percentage)
    {
        ThrowHelper.ThrowIf<ArgumentOutOfRangeException>(percentage is > 1 or < 0, "The percentage must between 0 and 1.");
        return Math.Abs(percentage - 1) < double.Epsilon ? 1 : 1 - Math.Pow(Exponent, -10 * percentage);
    }
}