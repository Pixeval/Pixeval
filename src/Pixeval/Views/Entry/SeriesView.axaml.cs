// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Pixeval.Utilities;
using Pixeval.ViewModels;
using Pixeval.Views.Viewers;

namespace Pixeval.Views.Entry;

public partial class SeriesView : UserControl
{
    public SeriesView() => InitializeComponent();

    public void SetViewModel(SeriesViewViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        var oldViewModel = DataContext as IDisposable;
        DataContext = viewModel;
        oldViewModel?.Dispose();
        if (IsLoaded)
            RegisterViewModelDisposal(viewModel);
    }

    private void SeriesItem_OnTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not Control { DataContext: SeriesItemViewModel viewModel })
            return;

        TopLevel.GetTopLevel(this)?.ViewContainer?.CreateSeriesPage(viewModel.WorkType, viewModel.Entry.Id);
    }

    private void SeriesListBox_OnContainerPrepared(object? sender, ContainerPreparedEventArgs e)
    {
        if (e.Container is ListBoxItem item)
            item.Tapped += SeriesItem_OnTapped;
    }

    private void SeriesListBox_OnContainerClearing(object? sender, ContainerClearingEventArgs e)
    {
        if (e.Container is ListBoxItem item)
            item.Tapped -= SeriesItem_OnTapped;
    }

    /// <inheritdoc />
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (DataContext is SeriesViewViewModel viewModel)
            RegisterViewModelDisposal(viewModel);
    }

    private void RegisterViewModelDisposal(SeriesViewViewModel viewModel) =>
        RaiseEvent(new ViewModelDisposalEventArgs(ViewModelDisposal.ViewModelDisposalEvent, viewModel));
}
