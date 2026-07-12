// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

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
    private static readonly ConditionalWeakTable<Control, DisposableRegistry> _DisposableRegistries = new();

    public static readonly RoutedEvent<ViewModelDisposalEventArgs> ViewModelDisposalEvent =
        RoutedEvent.Register<Control, ViewModelDisposalEventArgs>(
            nameof(ViewModelDisposalEvent),
            RoutingStrategies.Bubble);

    public static readonly RoutedEvent<RoutedEventArgs> RequestDisposeEvent =
        RoutedEvent.Register<Control, RoutedEventArgs>(
            nameof(RequestDisposeEvent),
            RoutingStrategies.Bubble);

    public static void Register(Control control, IDisposable disposable) =>
        _DisposableRegistries.GetValue(control, static _ => new()).Add(disposable);

    public static void Dispose(Control control)
    {
        if (!_DisposableRegistries.TryGetValue(control, out var registry))
            return;

        _ = _DisposableRegistries.Remove(control);
        registry.Dispose();
    }
}

internal sealed class DisposableRegistry : IDisposable
{
    private readonly HashSet<IDisposable> _disposables = new(ReferenceEqualityComparer.Instance);
    private bool _isDisposed;

    public void Add(IDisposable disposable)
    {
        if (_isDisposed)
        {
            disposable.Dispose();
            return;
        }

        _ = _disposables.Add(disposable);
    }

    public void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;
        foreach (var disposable in _disposables)
            disposable.Dispose();
        _disposables.Clear();
    }
}
