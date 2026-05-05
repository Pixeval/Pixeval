// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Pixeval.ViewModels;

public abstract class DetailedViewModelBase : ViewModelBase, INotifyDetailedPropertyChanged
{
    public event DetailedPropertyChangedEventHandler? DetailedPropertyChanged;

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
}

public class DetailedPropertyChangedEventArgs(string? propertyName) : PropertyChangedEventArgs(propertyName)
{
    public Type? Type { get; init; }

    public object? OldValue { get; init; }

    public object? NewValue { get; init; }

    public object? OldTag { get; init; }

    public object? NewTag { get; init; }
}

public delegate void DetailedPropertyChangedEventHandler(object? sender, DetailedPropertyChangedEventArgs e);

public interface INotifyDetailedPropertyChanged
{
    event DetailedPropertyChangedEventHandler? DetailedPropertyChanged;
}
