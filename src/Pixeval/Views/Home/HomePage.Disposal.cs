// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Linq;
using Avalonia.Interactivity;
using Pixeval.Utilities;

namespace Pixeval.Views.Home;

public sealed partial class HomePage : IDisposable
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
        ViewModel.PropertyChanged -= ViewModel_OnPropertyChanged;
        _activeCardControl?.CancelEdit();
        _activeCardControl = null;
        _selectedCard = null;

        foreach (var control in HomeGrid.Children.OfType<HomePageCardControl>().ToList())
            DisposeCardControl(control);

        HomeGrid.Children.Clear();
        GuideGrid.Children.Clear();
        DataContext = null;
    }
}
