// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.Input;
using Mako.Model;
using Pixeval.Controls;
using Pixeval.I18N;
using Pixeval.Utilities;
using Pixeval.Views.Work;

namespace Pixeval.ViewModels;

public partial class WorkEntryViewModel<T>
{
    // TODO 用更通用的收藏接口
    [RelayCommand(CanExecute = nameof(IsBookmarkSupported))]
    private Task BookmarkAsync(Control? parameter) => ChangeBookmarkAsync(!IsFavorite);

    [RelayCommand(CanExecute = nameof(CanToggleAuthorFollow))]
    private async Task FollowAuthorAsync(Control? parameter)
    {
        if (Entry is not WorkBase { User: { } author } || !CanToggleAuthorFollow(parameter))
            return;

        var followed = !IsAuthorFollowed;
        if (parameter?.FindAncestorOfType<WorkView>() is { } view)
        {
            await view.SetAuthorsFollowedAsync([author], followed);
            return;
        }

        if (await MakoHelper.SetFollowAsync(author, followed) == followed)
            OnPropertyChanged(nameof(IsAuthorFollowed));
    }

    private bool CanToggleAuthorFollow(Control? parameter) =>
        !IsBlocked && Entry is WorkBase { User: { Id: > 0 } author }
                   && WorkView.IsFollowableAuthor(author)
                   && parameter?.FindAncestorOfType<WorkView>()?.IsChangingAuthorFollows is not true;

    [RelayCommand(CanExecute = nameof(IsBookmarkSupported))]
    private Task AddToBookmarkAsync((IReadOnlyList<string>? Tags, bool IsPrivate, Control? Control) parameter) =>
        ChangeBookmarkAsync(true, parameter.IsPrivate, parameter.Tags);

    [RelayCommand(CanExecute = nameof(IsBookmarkSupported))]
    private Task RemoveFromBookmarkAsync() => IsFavorite ? ChangeBookmarkAsync(false) : Task.CompletedTask;

    private async Task ChangeBookmarkAsync(bool isFavorite, bool isPrivate = false, IReadOnlyList<string>? tags = null)
    {
        if (IsBlocked || (IsBookmarkedDisplay & HeartButtonState.Pending) is not 0)
            return;
        IsBookmarkedDisplay |= HeartButtonState.Pending; // pre-update
        try
        {
            _ = await SetBookmarkAsync(isFavorite, isPrivate, tags);
        }
        finally
        {
            IsBookmarkedDisplay = IsFavorite ? HeartButtonState.Checked : HeartButtonState.Unchecked;
        }
    }

    bool IWorkViewModel.TryAddToWatchLater() => SetWatchLater(true);

    [RelayCommand(CanExecute = nameof(CanManageWatchLater))]
    private void AddToWatchLater(Control? parameter)
    {
        var target = !IsInWatchLater;
        if (!SetWatchLater(target))
            return;

        TopLevel.GetTopLevel(parameter)?.ViewContainer?.ShowSuccess(
            I18NManager.GetResource(target ? MiscResources.AddedToWatchLater : MiscResources.RemovedFromWatchLater));
    }

    private bool SetWatchLater(bool include)
    {
        if (IsBlocked || GetHistoryPersistHelper() is not { } helper
            || !(include ? helper.AddWatchLater(Entry) : helper.RemoveWatchLater(Entry)))
            return false;

        IsInWatchLater = include;
        return true;
    }

    protected abstract Task<bool> SetBookmarkAsync(bool favorite, bool privately = false, IReadOnlyCollection<string>? tags = null);

    [RelayCommand]
    protected abstract Task SaveAsync(Control? parameter);
}
