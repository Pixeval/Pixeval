// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.ObjectModel;
using Mako.Global.Enum;
using Misaki;
using Pixeval.Controls;

namespace Pixeval.Views;

public abstract partial class HistoryPage : IconContentPage
{
    private bool _isSourceInitialized;

    protected HistoryPage() => InitializeComponent();

    private void SimpleWorkTypeComboBox_OnSelectionChanged(SymbolComboBox sender, EventArgs e)
    {
        if (!_isSourceInitialized)
            return;

        WorkContainer.SetSource(Source, sender.GetSelectedValue<SimpleWorkType>(), needRefreshOnOpen: true);
    }

    protected void InitializeSource()
    {
        _isSourceInitialized = true;
        SimpleWorkTypeComboBox_OnSelectionChanged(SimpleWorkTypeComboBox, EventArgs.Empty);
    }

    protected abstract ObservableCollection<IArtworkInfo> Source { get; }
}

public class BrowsingHistoryPage : HistoryPage
{
    public BrowsingHistoryPage() => InitializeSource();

    /// <inheritdoc />
    protected override ObservableCollection<IArtworkInfo> Source =>
        App.AppViewModel.HistoryPersistHelper.BrowseHistoryEntries;
}

public class WatchLaterPage : HistoryPage
{
    public WatchLaterPage() => InitializeSource();

    /// <inheritdoc />
    protected override ObservableCollection<IArtworkInfo> Source =>
        App.AppViewModel.HistoryPersistHelper.WatchLaterEntries;
}
