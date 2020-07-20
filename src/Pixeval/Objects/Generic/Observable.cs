#region Copyright (C) 2019-2020 Dylech30th. All rights reserved.

// Pixeval - A Strong, Fast and Flexible Pixiv Client
// Copyright (C) 2019-2020 Dylech30th
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using PropertyChanged;

namespace Pixeval.Objects.Generic
{
    [AddINotifyPropertyChangedInterface]
    public class Observable<T>
    {
        private T value;

        public Observable(T value)
        {
            this.value = value;
        }

        public T Value
        {
            get => value;
            set
            {
                if (!this.value.Equals(value))
                {
                    ValueChanged?.Invoke(Value, new ObservableValueChangedEventArgs<T>(this.value, value));
                    this.value = value;
                }
            }
        }

        public event EventHandler<ObservableValueChangedEventArgs<T>> ValueChanged;
    }

    public class ObservableValueChangedEventArgs<T> : EventArgs
    {
        public ObservableValueChangedEventArgs(T oldValue, T newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        public T OldValue { get; set; }

        public T NewValue { get; set; }
    }
}
