// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using Pixeval.Controls;

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
        if (App.AppViewModel.AppSettings.DownloadWhenBookmarked && result)
            await SaveAsync(parameter);
    }

    [RelayCommand(CanExecute = nameof(IsBookmarkSupported))]
    private async Task AddToBookmarkAsync((IEnumerable<string> UserTags, bool IsPrivate, Control? Control) parameter)
    {
        if ((IsBookmarkedDisplay & HeartButtonState.Pending) is not 0)
            return;
        IsBookmarkedDisplay |= HeartButtonState.Pending; // pre-update
        var result = await SetBookmarkAsync(true, parameter.IsPrivate, parameter.UserTags);
        IsBookmarkedDisplay = result ? HeartButtonState.Checked : HeartButtonState.Unchecked;
        if (App.AppViewModel.AppSettings.DownloadWhenBookmarked)
            await SaveAsync(parameter.Control);
    }

    protected abstract Task<bool> SetBookmarkAsync(bool favorite, bool privately = false, IEnumerable<string>? tags = null);

    [RelayCommand]
    protected abstract Task SaveAsync(Control? parameter);

    [RelayCommand]
    protected abstract Task SaveAsAsync(Control? parameter);
}
