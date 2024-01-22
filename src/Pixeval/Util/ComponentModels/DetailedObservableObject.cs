#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/DetailedObservableObject.cs
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

using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Pixeval.Util.ComponentModels;

public class DetailedObservableObject : ObservableObject, INotifyDetailedPropertyChanging, INotifyDetailedPropertyChanged
{
    public event DetailedPropertyChangedEventHandler? DetailedPropertyChanged;

    public event DetailedPropertyChangingEventHandler? DetailedPropertyChanging;

    /// <summary>
    /// Raises the <see cref = "ObservableObject.PropertyChanged"/> and <see cref="DetailedPropertyChanged"/> event.
    /// </summary>
    /// <param name="oldValue"></param>
    /// <param name="newValue"></param>
    /// <param name="oldTag"></param>
    /// <param name="newTag"></param>
    /// <param name = "propertyName">(optional) The name of the property that changed.</param>
    protected void OnDetailedPropertyChanged<T>(T oldValue, T newValue, object? oldTag = null, object? newTag = null, [CallerMemberName] string? propertyName = null)
    {
        var args = new DetailedPropertyChangedEventArgs(propertyName)
        {
            OldValue = oldValue,
            NewValue = newValue,
            OldTag = oldTag,
            NewTag = newTag,
            Type = typeof(T)
        };
        DetailedPropertyChanged?.Invoke(this, args);
        OnPropertyChanged(args);
    }

    /// <summary>
    /// Raises the <see cref = "ObservableObject.PropertyChanging"/> and <see cref="DetailedPropertyChanging"/> event.
    /// </summary>
    /// <param name="oldValue"></param>
    /// <param name="newValue"></param>
    /// <param name="oldTag"></param>
    /// <param name="newTag"></param>
    /// <param name = "propertyName">(optional) The name of the property that changed.</param>
    protected void OnDetailedPropertyChanging<T>(T oldValue, T newValue, object? oldTag = null, object? newTag = null, [CallerMemberName] string? propertyName = null)
    {
        var args = new DetailedPropertyChangingEventArgs(propertyName)
        {
            OldValue = oldValue,
            NewValue = newValue,
            OldTag = oldTag,
            NewTag = newTag,
            Type = typeof(T)
        };
        DetailedPropertyChanging?.Invoke(this, args);
        OnPropertyChanging(args);
    }
}
