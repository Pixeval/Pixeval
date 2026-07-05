// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Collections.ObjectModel;
using System;
using Avalonia.Interactivity;
using Avalonia.Controls;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using Mako.Global.Enum;
using Misaki;
using Pixeval.Controls;
using Pixeval.I18N;
using Pixeval.Utilities;
using Pixeval.ViewModels;

namespace Pixeval.Views;

public abstract partial class HistoryPage : ContentPage, IDisposable
{
    private SimpleOperableViewViewModel<IllustrationItemViewModel>? _illustrationViewModel;
    private SimpleOperableViewViewModel<NovelItemViewModel>? _novelViewModel;
    private bool _isDisposed;

    protected HistoryPage(string header, Symbol symbol)
    {
        Header = header;
        Icon = new SymbolIcon { Symbol = symbol, FontSize = 16, IconVariant = IconVariant.Color };
        InitializeComponent();
    }

    private void SimpleWorkTypeComboBox_OnSelectionChanged(SymbolComboBox sender, EventArgs e)
    {
        if (_illustrationViewModel is null || _novelViewModel is null)
            return;

        IOperableViewViewModel? viewModel = sender.GetSelectedValue<SimpleWorkType>() is SimpleWorkType.Novel
            ? _novelViewModel
            : _illustrationViewModel;
        // 保留viewModel到最后关闭时才dispose，避免切换时重新创建
        WorkContainer.SetViewModel(viewModel, ownsViewModel: false);
    }

    protected void SetSource()
    {
        _illustrationViewModel = new(Source, needRefreshOnOpen: true);
        _novelViewModel = new(Source, needRefreshOnOpen: true);
        SimpleWorkTypeComboBox_OnSelectionChanged(SimpleWorkTypeComboBox, EventArgs.Empty);
    }

    protected abstract ObservableCollection<IArtworkInfo> Source { get; }

    #region Disposal

    /// <inheritdoc />
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        RaiseEvent(new ViewModelDisposalEventArgs(ViewModelDisposal.ViewModelDisposalEvent, this));
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        if (_isDisposed)
            return;

        _isDisposed = true;
        _illustrationViewModel?.Dispose();
        _illustrationViewModel = null;
        _novelViewModel?.Dispose();
        _novelViewModel = null;
    }

    #endregion
}

public class BrowsingHistoryPage : HistoryPage
{
    public BrowsingHistoryPage() : base(I18NManager.GetResource(MainPageResources.TabBrowsingHistory), Symbol.History) => SetSource();

    /// <inheritdoc />
    protected override ObservableCollection<IArtworkInfo> Source =>
        App.AppViewModel.HistoryPersistHelper.BrowseHistoryEntries;
}


public class WatchLaterPage : HistoryPage
{
    public WatchLaterPage() : base(I18NManager.GetResource(MainPageResources.TabWatchLater), Symbol.Clock) => SetSource();

    /// <inheritdoc />
    protected override ObservableCollection<IArtworkInfo> Source =>
        App.AppViewModel.HistoryPersistHelper.WatchLaterEntries;
}
