// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using Pixeval.Controls;
using Pixeval.I18N;
using Pixeval.Utilities;

namespace Pixeval.ViewModels;

public partial class WorkEntryViewModel<T>
{
    // TODO 用更通用的收藏接口
    [RelayCommand(CanExecute = nameof(IsBookmarkSupported))]
    private async Task BookmarkAsync(Control? parameter)
    {
        if ((IsBookmarkedDisplay & HeartButtonState.Pending) is not 0)
            return;
        IsBookmarkedDisplay |= HeartButtonState.Pending; // pre-update
        var result = await SetBookmarkAsync(!IsFavorite);
        IsBookmarkedDisplay = result ? HeartButtonState.Checked : HeartButtonState.Unchecked;
    }

    [RelayCommand(CanExecute = nameof(IsBookmarkSupported))]
    private async Task AddToBookmarkAsync((IReadOnlyList<string>? Tags, bool IsPrivate, Control? Control) parameter)
    {
        if ((IsBookmarkedDisplay & HeartButtonState.Pending) is not 0)
            return;
        IsBookmarkedDisplay |= HeartButtonState.Pending; // pre-update
        var result = await SetBookmarkAsync(true, parameter.IsPrivate, parameter.Tags);
        IsBookmarkedDisplay = result ? HeartButtonState.Checked : HeartButtonState.Unchecked;
    }

    [RelayCommand(CanExecute = nameof(CanManageWatchLater))]
    private void AddToWatchLater(Control? parameter)
    {
        if (GetHistoryPersistHelper() is not { } helper)
            return;

        var target = !IsInWatchLater;

        if (target)
        {
            if (!helper.AddWatchLater(Entry))
                return;
        }
        else if (!helper.RemoveWatchLater(Entry))
            return;

        IsInWatchLater = target;

        TopLevel.GetTopLevel(parameter)?.ViewContainer?.ShowSuccess(
            I18NManager.GetResource(target ? MiscResources.AddedToWatchLater : MiscResources.RemovedFromWatchLater));
    }

    protected abstract Task<bool> SetBookmarkAsync(bool favorite, bool privately = false, IReadOnlyCollection<string>? tags = null);

    [RelayCommand]
    protected abstract Task SaveAsync(Control? parameter);

    [RelayCommand]
    protected abstract Task SaveAsAsync(Control? parameter);
}
