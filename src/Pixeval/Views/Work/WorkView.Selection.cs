// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mako.Global.Enum;
using Mako.Model;
using Pixeval.Controls;
using Pixeval.I18N;
using Pixeval.Models.Blocking;
using Pixeval.Utilities;
using Pixeval.ViewModels;

namespace Pixeval.Views.Work;

public sealed partial class WorkView
{
    internal SelectionMenuModel SelectionMenu { get; }

    internal bool IsChangingAuthorFollows
    {
        get;
        private set
        {
            field = value;
            SelectionMenu.FollowCommand.NotifyCanExecuteChanged();
            SelectionMenu.UnfollowCommand.NotifyCanExecuteChanged();
        }
    }

    internal IReadOnlyList<IWorkViewModel> GetSelectedWorks() =>
        DataContext is IOperableViewViewModel viewModel ? [.. viewModel.SelectedEntries] : [];

    private void SelectionMenu_OnOpening(object? sender, CancelEventArgs e)
    {
        if (!WorkListBox.HasMultipleSelection || GetSelectedWorks() is not { Count: > 1 } works)
        {
            e.Cancel = true;
            return;
        }

        var authors = GetWorkAuthors(works).ToArray();
        SelectionMenu.BookmarkCount = works.Count(work => IsBookmarkTarget(work, bookmarked: true));
        SelectionMenu.RemoveBookmarkCount = works.Count(work => IsBookmarkTarget(work, bookmarked: false));
        SelectionMenu.FollowCount = authors.Count(author => IsFollowTarget(author, followed: true));
        SelectionMenu.UnfollowCount = authors.Count(author => IsFollowTarget(author, followed: false));
        SelectionMenu.CanSave = works.Any(CanSave);
        SelectionMenu.CanBlockAuthors = authors.Length > 0;
    }

    private async void SelectionBookmark_OnContextRequested(object? sender, ContextRequestedEventArgs e)
    {
        var works = GetSelectedWorks();
        if (!works.Any(work => IsBookmarkTarget(work, bookmarked: true)))
            return;

        e.Handled = true;
        WorkListBox.ContextMenu?.Close();
        await ShowBookmarkTagSelectorAsync(WorkListBox, works, onlyUnbookmarked: true);
    }

    internal Task ShowBookmarkTagSelectorAsync(
        Control placementTarget, IReadOnlyList<IWorkViewModel> works, long workId = 0, bool onlyUnbookmarked = false)
    {
        var supportedWorks = works.Where(work => onlyUnbookmarked ? IsBookmarkTarget(work, bookmarked: true) : work.IsBookmarkSupported)
            .ToArray();
        if (supportedWorks.Length is 0)
            return Task.CompletedTask;

        var type = supportedWorks[0] is NovelItemViewModel ? SimpleWorkType.Novel : SimpleWorkType.Illustration;
        return BookmarkTagSelectorFlyoutHelper.ShowAsync(
            placementTarget, type, workId, options => AddWorksToBookmarkAsync(supportedWorks, options, onlyUnbookmarked));
    }

    private Task AddWorksToBookmarkAsync(
        IReadOnlyList<IWorkViewModel> works, (bool IsPrivate, IReadOnlyList<string>? Tags) options, bool onlyUnbookmarked = false) =>
        SetWorksBookmarkAsync(works, bookmarked: true, skipUnchanged: onlyUnbookmarked, options);

    private async Task SetWorksBookmarkAsync(
        IReadOnlyList<IWorkViewModel> works, bool bookmarked, bool skipUnchanged, (bool IsPrivate, IReadOnlyList<string>? Tags) options = default)
    {
        var parameter = (options.Tags, options.IsPrivate, (Control?) this);
        var targets = works.Where(CanChangeBookmark).ToArray();
        if (targets.Length is 0)
            return;

        var viewContainer = TopLevel.GetTopLevel(this)?.ViewContainer;
        if (viewContainer is not null && targets.Length >= 20
            && await viewContainer.CreateOkCancelAsync(
                I18NManager.GetResource(bookmarked
                    ? WorkContainerResources.SelectedTooManyItems.ForBookmarkTitle
                    : WorkContainerResources.SelectedTooManyItems.ForRemoveBookmarkTitle),
                I18NManager.GetResource(WorkContainerResources.SelectedTooManyItems.Content)) is not ContentDialogResult.Primary)
            return;

        var succeeded = 0;
        foreach (var work in targets)
        {
            if (!CanChangeBookmark(work))
                continue;

            if (bookmarked)
                await work.AddToBookmarkCommand.ExecuteAsync(parameter);
            else
                await work.RemoveFromBookmarkCommand.ExecuteAsync(null);
            if (work.Entry.IsFavorite == bookmarked)
                succeeded++;
        }

        if (succeeded > 0)
            viewContainer?.ShowSuccess(bookmarked && targets.Length is 1
                ? I18NManager.GetResource(MiscResources.AddedToBookmark)
                : I18NManager.GetResource(bookmarked
                    ? WorkContainerResources.AddedAllToBookmarkContentFormatted
                    : WorkContainerResources.RemovedFromBookmarkFormatted, succeeded));
        return;

        bool CanChangeBookmark(IWorkViewModel work) => (!skipUnchanged || IsBookmarkTarget(work, bookmarked))
            && (bookmarked ? work.AddToBookmarkCommand.CanExecute(parameter) : work.RemoveFromBookmarkCommand.CanExecute(null));
    }

    private static bool IsBookmarkTarget(IWorkViewModel work, bool bookmarked) =>
        work.IsBookmarkSupported && work.Entry.IsFavorite != bookmarked;

    private async Task SaveWorksAsync(IReadOnlyList<IWorkViewModel> works)
    {
        var targets = works.Where(CanSave).ToArray();
        if (targets.Length is 0)
            return;

        var viewContainer = TopLevel.GetTopLevel(this)?.ViewContainer;
        if (viewContainer is not null && targets.Length >= 20
            && await viewContainer.CreateOkCancelAsync(
                I18NManager.GetResource(WorkContainerResources.SelectedTooManyItems.ForSaveTitle),
                I18NManager.GetResource(WorkContainerResources.SelectedTooManyItems.Content)) is not ContentDialogResult.Primary)
            return;

        var queued = 0;
        foreach (var work in targets)
        {
            if (!CanSave(work))
                continue;

            await work.SaveCommand.ExecuteAsync(null);
            queued++;
        }

        if (queued > 0)
            viewContainer?.ShowInformation(I18NManager.GetResource(WorkContainerResources.DownloadItemsQueuedFormatted, queued));
    }

    private static bool CanSave(IWorkViewModel work) =>
        !BlockedContentHelper.IsBlockedPlaceholder(work.Entry) && work.SaveCommand.CanExecute(null);

    private async Task OpenWorksInBrowserAsync(IReadOnlyList<IWorkViewModel> works)
    {
        if (TopLevel.GetTopLevel(this) is not { Launcher: { } launcher } topLevel)
            return;

        if (topLevel.ViewContainer is { } viewContainer && works.Count > 15
            && await viewContainer.CreateOkCancelAsync(
                I18NManager.GetResource(WorkContainerResources.SelectedTooManyItems.ForOpenInBrowser.Title),
                I18NManager.GetResource(WorkContainerResources.SelectedTooManyItems.ForOpenInBrowser.Content)) is not ContentDialogResult.Primary)
            return;

        foreach (var work in works)
            _ = await launcher.LaunchUriAsync(work.Entry.WebsiteUri);
    }

    private void AddWorksToWatchLater(IReadOnlyList<IWorkViewModel> works)
    {
        var added = works.Count(work => work.TryAddToWatchLater());

        if (added > 0)
            TopLevel.GetTopLevel(this)?.ViewContainer?.ShowSuccess(
                I18NManager.GetResource(WorkContainerResources.AddedToWatchLaterFormatted, added));
    }

    private static UserInfo? GetWorkAuthor(IWorkViewModel work) =>
        work.Entry is WorkBase { User: { Id: > 0 } author } && !BlockedContentHelper.IsBlockedPlaceholder(work.Entry) ? author : null;

    private static IEnumerable<UserInfo> GetWorkAuthors(IReadOnlyList<IWorkViewModel> works) =>
        works.Select(GetWorkAuthor).OfType<UserInfo>()
            .GroupBy(author => author.Id)
            .Select(group => group.FirstOrDefault(author => author.IsFollowed) ?? group.First());

    internal static bool IsFollowableAuthor(UserInfo author) => author.Id != App.AppViewModel?.MakoClient.Me?.Id;

    private static bool IsFollowTarget(UserInfo author, bool followed) => author.IsFollowed != followed && IsFollowableAuthor(author);

    internal async Task SetAuthorsFollowedAsync(IEnumerable<UserInfo> authors, bool followed)
    {
        if (IsChangingAuthorFollows)
            return;

        var targets = authors.Where(author => IsFollowTarget(author, followed)).ToArray();
        if (targets.Length is 0)
            return;

        IsChangingAuthorFollows = true;
        try
        {
            var loadedWorks = (DataContext as IOperableViewViewModel)?.Source
                .Select(work => (Work: work, Author: GetWorkAuthor(work)))
                .Where(pair => pair.Author is not null)
                .ToLookup(pair => pair.Author!.Id, pair => pair.Work);
            var succeeded = 0;
            foreach (var author in targets)
            {
                if (await MakoHelper.SetFollowAsync(author, followed) != followed)
                    continue;

                succeeded++;
                if (loadedWorks is not null)
                    foreach (var loadedWork in loadedWorks[author.Id])
                        loadedWork.SetAuthorFollowed(followed);
            }

            var viewContainer = TopLevel.GetTopLevel(this)?.ViewContainer;
            if (succeeded > 0)
                viewContainer?.ShowSuccess(I18NManager.GetResource(followed
                    ? WorkContainerResources.FollowedAuthorsFormatted
                    : WorkContainerResources.UnfollowedAuthorsFormatted, succeeded));
            if (succeeded < targets.Length)
                viewContainer?.ShowWarning(I18NManager.GetResource(followed
                    ? WorkContainerResources.FollowAuthorsFailedFormatted
                    : WorkContainerResources.UnfollowAuthorsFailedFormatted, targets.Length - succeeded));
        }
        finally
        {
            IsChangingAuthorFollows = false;
        }
    }

    private void BlockAuthors(IReadOnlyList<UserInfo> authors)
    {
        if (authors.Count is 0)
            return;

        var added = authors.Count(BlockedContentHelper.TryAddOrUpdateBlockedUser);
        // Refresh works for authors already blocked from another page as well.
        (DataContext as IOperableViewViewModel)?.ReplaceBlockedEntries();

        if (added > 0)
            TopLevel.GetTopLevel(this)?.ViewContainer?.ShowSuccess(authors.Count is 1
                ? I18NManager.GetResource(BlockedContentResources.AuthorBlockedFormatted, authors[0].Name)
                : I18NManager.GetResource(WorkContainerResources.BlockedAuthorsFormatted, added));
    }

    internal sealed partial class SelectionMenuModel(WorkView view) : ObservableObject
    {
        [ObservableProperty]
        public partial int BookmarkCount { get; set; }

        [ObservableProperty]
        public partial int RemoveBookmarkCount { get; set; }

        [ObservableProperty]
        public partial int FollowCount { get; set; }

        [ObservableProperty]
        public partial int UnfollowCount { get; set; }

        [ObservableProperty]
        public partial bool CanSave { get; set; }

        [ObservableProperty]
        public partial bool CanBlockAuthors { get; set; }

        [RelayCommand]
        private Task BookmarkAsync() => view.AddWorksToBookmarkAsync(view.GetSelectedWorks(), (false, null), onlyUnbookmarked: true);

        [RelayCommand]
        private Task BookmarkAllAsync() => view.AddWorksToBookmarkAsync(view.GetSelectedWorks(), (false, null));

        [RelayCommand]
        private Task RemoveBookmarkAsync() => view.SetWorksBookmarkAsync(view.GetSelectedWorks(), bookmarked: false, skipUnchanged: true);

        [RelayCommand]
        private void AddToWatchLater() => view.AddWorksToWatchLater(view.GetSelectedWorks());

        [RelayCommand]
        private Task SaveAsync() => view.SaveWorksAsync(view.GetSelectedWorks());

        [RelayCommand]
        private Task OpenInBrowserAsync() => view.OpenWorksInBrowserAsync(view.GetSelectedWorks());

        [RelayCommand(CanExecute = nameof(CanChangeAuthorFollows))]
        private Task FollowAsync() => view.SetAuthorsFollowedAsync(GetWorkAuthors(view.GetSelectedWorks()), true);

        [RelayCommand(CanExecute = nameof(CanChangeAuthorFollows))]
        private Task UnfollowAsync() => view.SetAuthorsFollowedAsync(GetWorkAuthors(view.GetSelectedWorks()), false);

        private bool CanChangeAuthorFollows() => !view.IsChangingAuthorFollows;

        [RelayCommand]
        private void BlockAuthors() => view.BlockAuthors([.. GetWorkAuthors(view.GetSelectedWorks())]);
    }
}
