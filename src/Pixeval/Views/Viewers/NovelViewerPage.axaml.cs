// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Mako.Global.Enum;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.Controls;
using Pixeval.I18N;
using Pixeval.Models.Extensions;
using Pixeval.Utilities;
using Pixeval.ViewModels.Viewers;
using Pixeval.Views.Work;

namespace Pixeval.Views.Viewers;

public partial class NovelViewerPage : IconContentPage
{
    private NovelViewerPageViewModel ViewModel => (NovelViewerPageViewModel) DataContext!;

    private readonly List<ExtensionsHostModel> _observedExtensionHosts = [];

    private static ExtensionService ExtensionService =>
        App.AppViewModel.AppServiceProvider.GetRequiredService<ExtensionService>();

    public NovelViewerPage() : this(null)
    {
    }

    public NovelViewerPage(NovelViewerPageViewModel? viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        _ = KeyboardShortcut.TryExecute(e, Key.Left, ViewModel.PrevCommand)
            || KeyboardShortcut.TryExecute(e, Key.Right, ViewModel.NextCommand)
            || KeyboardShortcut.TryExecute(e, Key.Up, ViewModel.PrevWorkCommand)
            || KeyboardShortcut.TryExecute(e, Key.Down, ViewModel.NextWorkCommand);
    }

    private void PrevButton_OnRightClick(object? sender, ContextRequestedEventArgs e) => ViewModel.PrevWorkCommand.Execute(null);

    private void NextButton_OnRightClick(object? sender, ContextRequestedEventArgs e) => ViewModel.NextWorkCommand.Execute(null);

    private void MarkdownBox_OnHyperlinkClicked(object? sender, MarkdownHyperlinkClickedEventArgs e)
    {
        if (!TryParsePageLink(e.Url, out var pageIndex))
            return;

        e.Handled = true;

        if (pageIndex >= 0 && pageIndex < ViewModel.PageCount)
            ViewModel.CurrentPageIndex = pageIndex;
    }

    private async void AddToBookmarkButton_OnClick(object? sender, ContextRequestedEventArgs e)
    {
        if (sender is Control c && ViewModel.CurrentNovel is { Entry.Id: var id })
            await BookmarkTagSelectorFlyoutHelper.ShowAsync(
                c,
                SimpleWorkType.Novel,
                id,
                AddToBookmarkAsync,
                PlacementMode.Bottom);
    }

    private async Task AddToBookmarkAsync((bool IsPrivate, IReadOnlyList<string>? Tags) e)
    {
        if (ViewModel.CurrentNovel is { } current)
        {
            await current.AddToBookmarkCommand.ExecuteAsync((e.Tags, e.IsPrivate, this));
            TopLevel.GetTopLevel(this)?.ViewContainer?.ShowSuccess(
                I18NManager.GetResource(MiscResources.AddedToBookmark));
        }
    }

    private static bool TryParsePageLink(string url, out int pageIndex)
    {
        pageIndex = 0;
        var link = url.StartsWith('#') ? url[1..] : url;

        if (!link.StartsWith("page", StringComparison.OrdinalIgnoreCase))
            return false;

        return int.TryParse(link[4..], out pageIndex);
    }

    #region Disposal

    /// <inheritdoc />
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        ExtensionService.HostModels.CollectionChanged += ExtensionHostsOnCollectionChanged;
        ObserveExtensionHosts();
        RaiseEvent(new ViewModelDisposalEventArgs(ViewModelDisposal.ViewModelDisposalEvent, ViewModel));
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        ExtensionService.HostModels.CollectionChanged -= ExtensionHostsOnCollectionChanged;
        foreach (var host in _observedExtensionHosts)
            host.PropertyChanged -= ExtensionHostOnPropertyChanged;
        _observedExtensionHosts.Clear();
        base.OnUnloaded(e);
    }

    private void ExtensionHostsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => ObserveExtensionHosts();

    private void ObserveExtensionHosts()
    {
        foreach (var host in _observedExtensionHosts)
            host.PropertyChanged -= ExtensionHostOnPropertyChanged;
        _observedExtensionHosts.Clear();
        _observedExtensionHosts.AddRange(ExtensionService.HostModels);
        foreach (var host in _observedExtensionHosts)
            host.PropertyChanged += ExtensionHostOnPropertyChanged;
        ViewModel.RefreshTranslationAvailability();
    }

    private void ExtensionHostOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ExtensionsHostModel.IsActive))
            ViewModel.RefreshTranslationAvailability();
    }

    #endregion
}
