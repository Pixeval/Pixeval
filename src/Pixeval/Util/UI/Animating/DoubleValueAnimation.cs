#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/DoubleValueAnimation.cs
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

public class DoubleValueAnimation : AbstractValueAnimation<double>
{
    public DoubleValueAnimation(
        TimeSpan duration,
        double from, 
        double to,
        IEasingFunction<double>? easingFunction = null) : this(duration, TimeSpan.FromMilliseconds(10), from, to, easingFunction)
    {
        
    }

    public DoubleValueAnimation(TimeSpan duration,
        TimeSpan sampleRate,
        double from, 
        double to,
        IEasingFunction<double>? easingFunction)
    {
        Duration = duration;
        SampleRate = sampleRate;
        From = from;
        To = to;
        EasingFunction = easingFunction;
    }

    public override TimeSpan Duration { get; }

    public override TimeSpan SampleRate { get; }

    public override double From { get; }

    public override double To { get; }

    public override IEasingFunction<double>? EasingFunction { get; }
}