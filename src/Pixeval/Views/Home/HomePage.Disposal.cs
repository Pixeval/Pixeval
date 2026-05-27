// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Linq;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Pixeval.Utilities;

namespace Pixeval.Views.Home;

public partial class HomePage : IDisposable
{
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        RaiseEvent(new ViewModelDisposalEventArgs(ViewModelDisposal.ViewModelDisposalEvent, this));
    }

    public void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;
        GC.SuppressFinalize(this);
        _activeCardControl?.CancelEdit();
        _activeCardControl = null;
        _selectedCard = null;
        _pendingTemplate = null;

        foreach (var control in HomeGrid.Children.OfType<HomePageCardControl>().ToList())
            DisposeCardControl(control);

        HomeGrid.Children.Clear();
        GuideGrid.Children.Clear();
        CardLibraryItemsControl.ItemsSource = null;
    }

    ~HomePage() => Dispatcher.UIThread.InvokeAsync(Dispose);
}
