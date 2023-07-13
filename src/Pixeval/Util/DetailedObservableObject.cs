using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Pixeval.Util;

public class DetailedObservableObject : ObservableObject
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

public delegate void DetailedPropertyChangedEventHandler(object? sender, DetailedPropertyChangedEventArgs e);

public delegate void DetailedPropertyChangingEventHandler(object? sender, DetailedPropertyChangingEventArgs e);

public class DetailedPropertyChangedEventArgs : PropertyChangedEventArgs
{
    public DetailedPropertyChangedEventArgs(string? propertyName) : base(propertyName)
    {
    }

    public Type? Type { get; init; }

    public object? OldValue { get; init; }

    public object? NewValue { get; init; }

    public object? OldTag { get; init; }

    public object? NewTag { get; init; }
}

public class DetailedPropertyChangingEventArgs : PropertyChangingEventArgs
{
    public DetailedPropertyChangingEventArgs(string? propertyName) : base(propertyName)
    {
    }

    public Type? Type { get; init; }

    public object? OldValue { get; init; }

    public object? NewValue { get; init; }

    public object? OldTag { get; init; }

    public object? NewTag { get; init; }
}
