using System;
using System.ComponentModel;

namespace Pixeval.Util.ComponentModels;

public class DetailedPropertyChangedEventArgs(string? propertyName) : PropertyChangedEventArgs(propertyName)
{
    public Type? Type { get; init; }

    public object? OldValue { get; init; }

    public object? NewValue { get; init; }

    public object? OldTag { get; init; }

    public object? NewTag { get; init; }
}
