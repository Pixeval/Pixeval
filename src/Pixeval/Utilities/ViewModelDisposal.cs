using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Pixeval.Utilities;

public sealed class ViewModelDisposalEventArgs(
    RoutedEvent<ViewModelDisposalEventArgs> routedEvent,
    IDisposable disposable)
    : RoutedEventArgs(routedEvent)
{
    public IDisposable Disposable { get; } = disposable;
}

public static class ViewModelDisposal
{
    public static readonly RoutedEvent<ViewModelDisposalEventArgs> ViewModelDisposalEvent =
        RoutedEvent.Register<Control, ViewModelDisposalEventArgs>(
            nameof(ViewModelDisposalEvent),
            RoutingStrategies.Bubble);

    public static readonly RoutedEvent<RoutedEventArgs> RequestDisposeEvent =
        RoutedEvent.Register<Control, RoutedEventArgs>(
            nameof(RequestDisposeEvent),
            RoutingStrategies.Bubble);

    public static Dictionary<Control, HashSet<WeakReference<IDisposable>>> DisposableMap { get; } = [];

    extension(Dictionary<Control, HashSet<WeakReference<IDisposable>>> disposableMap)
    {
        public HashSet<WeakReference<IDisposable>> GetValueOrAddNew(Control control)
        {
            if (!disposableMap.TryGetValue(control, out var set))
            {
                set = new(WeakReferenceEqualityComparer.Instance);
                disposableMap.Add(control, set);
            }
            return set;
        }
    }
}

file class WeakReferenceEqualityComparer : IEqualityComparer<WeakReference<IDisposable>>
{
    public static WeakReferenceEqualityComparer Instance { get; } = new();

    public bool Equals(WeakReference<IDisposable>? x, WeakReference<IDisposable>? y)
    {
        if (x is null || y is null)
            return false;
        return ReferenceEquals(x.TryGetTarget(out var xTarget) ? xTarget : null, y.TryGetTarget(out var yTarget) ? yTarget : null);
    }

    public int GetHashCode(WeakReference<IDisposable> obj)
    {
        return obj.TryGetTarget(out var target) ? RuntimeHelpers.GetHashCode(target) : 0;
    }
}
