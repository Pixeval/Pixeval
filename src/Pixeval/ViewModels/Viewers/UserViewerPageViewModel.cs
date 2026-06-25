// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mako.Global.Enum;
using Mako.Net.Responses;
using Pixeval.Views.Capability;

namespace Pixeval.ViewModels.Viewers;

public partial class UserViewerPageViewModel : ViewModelBase, IDisposable
{
    private readonly CancellationTokenSource _loadingCts = new();

    [ObservableProperty]
    public partial bool IsFollowed { get; set; }

    [ObservableProperty]
    public partial bool IsLoading { get; private set; }

    [ObservableProperty]
    public partial string? LoadErrorMessage { get; private set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Id))]
    [NotifyPropertyChangedFor(nameof(AvatarUrl))]
    [NotifyPropertyChangedFor(nameof(BackgroundUrl))]
    [NotifyPropertyChangedFor(nameof(TabPages))]
    public partial SingleUserResponse? UserDetail { get; private set; }

    public long Id => UserDetail?.UserEntity.Id ?? field;

    public string? AvatarUrl => UserDetail?.UserEntity.ProfileImageUrls.Medium;

    public string? BackgroundUrl => UserDetail?.UserProfile.BackgroundImageUrl ?? AvatarUrl;

    public IReadOnlyList<ContentPage> TabPages => UserDetail is { UserEntity: var user }
        ?
        [
            new WorkPostsPage(user),
            new WorkBookmarksPage(user),
            new UserFollowingPage(Id),
            new UserMyPixivPage(Id),
            new RelatedUsersPage(Id),
        ]
        : [];

    public UserViewerPageViewModel(SingleUserResponse userDetail)
    {
        Id = userDetail.UserEntity.Id;
        UserDetail = userDetail;
    }

    public UserViewerPageViewModel(long userId)
    {
        Id = userId;
        _ = LoadUserAsync(userId);
    }

    partial void OnUserDetailChanged(SingleUserResponse? value)
    {
        if (value is not null)
            IsFollowed = value.UserEntity.IsFollowed;

        FollowCommand.NotifyCanExecuteChanged();
        FollowPrivatelyCommand.NotifyCanExecuteChanged();
        UnfollowCommand.NotifyCanExecuteChanged();
    }

    private async Task LoadUserAsync(long userId)
    {
        var token = _loadingCts.Token;

        IsLoading = true;
        LoadErrorMessage = null;
        try
        {
            var userDetail = await App.AppViewModel.MakoClient.GetUserFromIdAsync(userId);
            token.ThrowIfCancellationRequested();
            if (_disposed)
                return;

            UserDetail = userDetail;
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception e)
        {
            if (!token.IsCancellationRequested)
                LoadErrorMessage = e.Message;
        }
        finally
        {
            if (!token.IsCancellationRequested && !_disposed)
                IsLoading = false;
        }
    }

    private bool CanFollow => Id != App.AppViewModel.PixivUid;

    [RelayCommand(CanExecute = nameof(CanFollow))]
    private async Task FollowAsync()
    {
        var result = await App.AppViewModel.MakoClient.PostFollowUserAsync(Id, PrivacyPolicy.Public);
        if (result)
        {
            UserDetail?.UserEntity.IsFollowed = true;
            IsFollowed = true;
        }
    }

    [RelayCommand(CanExecute = nameof(CanFollow))]
    private async Task FollowPrivatelyAsync()
    {
        var result = await App.AppViewModel.MakoClient.PostFollowUserAsync(Id, PrivacyPolicy.Private);
        if (result)
        {
            UserDetail?.UserEntity.IsFollowed = true;
            IsFollowed = true;
        }
    }

    [RelayCommand(CanExecute = nameof(CanFollow))]
    private async Task UnfollowAsync()
    {
        var result = await App.AppViewModel.MakoClient.RemoveFollowUserAsync(Id);
        if (result)
        {
            UserDetail?.UserEntity.IsFollowed = false;
            IsFollowed = false;
        }
    }

    #region Dispose

    private bool _disposed;

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        if (_disposed)
            return;

        _disposed = true;
        _loadingCts.Cancel();
        _loadingCts.Dispose();
    }

    ~UserViewerPageViewModel() => Dispose();

    #endregion
}
