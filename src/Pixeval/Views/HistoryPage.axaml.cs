// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Mako.Global.Enum;
using Pixeval.Controls;
using Pixeval.Models.Database.Managers;
using Pixeval.Utilities;

namespace Pixeval.Views;

public abstract partial class HistoryPage : IconContentPage, IDisposable
{
    private bool _isDisposed;
    private bool _isSourceInitialized;

    protected HistoryPage() => InitializeComponent();

    private void SimpleWorkTypeComboBox_OnSelectionChanged(SymbolComboBox sender, EventArgs e)
    {
        if (!_isSourceInitialized)
            return;

        ResetSource(sender.GetSelectedValue<SimpleWorkType>());
    }

    private void WorkContainer_OnRefreshRequested(object? sender, RoutedEventArgs e) => ResetSource();

    protected void InitializeSource()
    {
        if (_isSourceInitialized)
            return;

        _isSourceInitialized = true;
        Source.Changed += Source_OnChanged;
        SimpleWorkTypeComboBox_OnSelectionChanged(SimpleWorkTypeComboBox, EventArgs.Empty);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (_isDisposed)
            return;

        RaiseEvent(new ViewModelDisposalEventArgs(ViewModelDisposal.ViewModelDisposalEvent, this));
    }

    private void Source_OnChanged(object? sender, EventArgs e) =>
        Dispatcher.UIThread.Post(() =>
        {
            if (!_isDisposed)
                ResetSource();
        });

    private void ResetSource(SimpleWorkType? workType = null) =>
        WorkContainer.ResetEngine(Source.StreamAsync(
            workType ?? SimpleWorkTypeComboBox.GetSelectedValue<SimpleWorkType>()));

    /// <inheritdoc />
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        if (_isDisposed)
            return;

        _isDisposed = true;
        Source.Changed -= Source_OnChanged;
    }

    protected abstract IArtworkHistorySource Source { get; }
}

public class BrowsingHistoryPage : HistoryPage
{
    public BrowsingHistoryPage() => InitializeSource();

    /// <inheritdoc />
    protected override IArtworkHistorySource Source =>
        App.AppViewModel.HistoryPersistHelper.BrowseHistorySource;
}

public class WatchLaterPage : HistoryPage
{
    public WatchLaterPage() => InitializeSource();

    /// <inheritdoc />
    protected override IArtworkHistorySource Source =>
        App.AppViewModel.HistoryPersistHelper.WatchLaterSource;
}
