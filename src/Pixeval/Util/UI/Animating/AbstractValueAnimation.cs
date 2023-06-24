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

public abstract class AbstractValueAnimation<V> : IValueAnimation<V> where V : INumber<V>
{
    public abstract TimeSpan Duration { get; }

    public abstract TimeSpan SampleRate { get; }

    public abstract V From { get; }

    public abstract V To { get; }

    public abstract IEasingFunction<V>? EasingFunction { get; }

    private Action<V>? _onValueChanged;

    public event Action<V> OnValueChanged
    {
        add => _onValueChanged += value;
        remove => _onValueChanged -= value;
    }

    private EventHandler? _onCompleted;

    public event EventHandler OnCompleted
    {
        add => _onCompleted += value;
        remove => _onCompleted -= value;
    }

    public Task StartAsync()
    {
        return ValueAnimations.StartAsync(this)
            .ForEachAsync(e => _onValueChanged?.Invoke(e))
            .ContinueWith(_ => _onCompleted?.Invoke(this, EventArgs.Empty));
    }
}