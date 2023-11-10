#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/AbstractValueAnimation.cs
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
