// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Mako.Global.Enum;
using Pixeval.Controls;
using Pixeval.I18N;
using Pixeval.Utilities;
using Pixeval.ViewModels.Viewers;
using Pixeval.Views.Work;

namespace Pixeval.Views.Viewers;

public partial class NovelViewerPage : ContentPage
{
    private NovelViewerPageViewModel ViewModel => (NovelViewerPageViewModel) DataContext!;

    public NovelViewerPage() : this(null)
    {
    }

    public NovelViewerPage(NovelViewerPageViewModel? viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
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

    private async void SaveButton_OnRightClick(object? sender, ContextRequestedEventArgs e)
    {
        if (ViewModel.CurrentNovel is { } current)
            await current.SaveAsCommand.ExecuteAsync(sender);
    }

    private async void AddToBookmarkButton_OnClick(object? sender, ContextRequestedEventArgs e)
    {
        if (sender is Control c)
            await BookmarkTagSelectorFlyoutHelper.ShowAsync(
                c,
                SimpleWorkType.Novel,
                AddToBookmarkAsync,
                PlacementMode.Bottom);
    }

    private async Task AddToBookmarkAsync((bool IsPrivate, IReadOnlyList<string>? Tags) e)
    {
        if (ViewModel.CurrentNovel is { } current)
        {
            await current.AddToBookmarkCommand.ExecuteAsync((e.Tags, e.IsPrivate, this));
            TopLevel.GetTopLevel(this)?.ViewContainer?.ShowSuccess(
                I18NManager.GetResource(EntryViewerPageResources.AddedToBookmark));
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

        RaiseEvent(new ViewModelDisposalEventArgs(ViewModelDisposal.ViewModelDisposalEvent, ViewModel));
    }

    #endregion
}
