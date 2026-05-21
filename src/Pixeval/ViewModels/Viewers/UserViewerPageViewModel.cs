// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mako.Global.Enum;
using Mako.Net.Response;
using Misaki;
using Pixeval.Utilities.IO.Caching;
using Pixeval.Views.Capability;

namespace Pixeval.ViewModels.Viewers;

public partial class UserViewerPageViewModel : ViewModelBase, IDisposable
{
    [ObservableProperty]
    public partial Bitmap? AvatarSource { get; set; }

    [ObservableProperty]
    public partial Bitmap? BackgroundSource { get; set; }

    [ObservableProperty]
    public partial bool IsFollowed { get; set; }

    public SingleUserResponse UserDetail { get; }

    public string Name => UserDetail.UserEntity.Name;

    public Profile Metrics => UserDetail.UserProfile;

    public long Id => UserDetail.UserEntity.Id;

    public string Account => UserDetail.UserEntity.Account;

    public bool IsPremium => UserDetail.UserProfile.IsPremium;

    public string Description => UserDetail.UserEntity.Description;

    public Uri WebsiteUri => UserDetail.UserEntity.WebsiteUri;

    public Uri AppUri => UserDetail.UserEntity.AppUri;

    private string AvatarUrl => UserDetail.UserEntity.ProfileImageUrls.Medium;

    private string? BackgroundUrl => UserDetail.UserProfile.BackgroundImageUrl;

    public IReadOnlyList<ContentPage> TabPages { get; }

    public UserViewerPageViewModel(SingleUserResponse userDetail)
    {
        UserDetail = userDetail;
        IsFollowed = userDetail.UserEntity.IsFollowed;

        TabPages =
        [
            new UserWorkPostsPage(UserDetail.UserEntity),
            new BookmarksPage(UserDetail.UserEntity),
            new FollowingsPage(Id),
            new MyPixivUsersPage(Id),
            new RelatedUsersPage(Id),
        ];

        _ = SetAvatarAndBackgroundAsync();
    }

    private async Task SetAvatarAndBackgroundAsync()
    {
        AvatarSource = await CacheHelper.GetBitmapAsync(IPlatformInfo.Pixiv, AvatarUrl);
        BackgroundSource = BackgroundUrl is not null
            ? await CacheHelper.GetBitmapAsync(IPlatformInfo.Pixiv, BackgroundUrl)
            : AvatarSource;
    }

    private bool CanFollow => Id != App.AppViewModel.PixivUid;

    [RelayCommand(CanExecute = nameof(CanFollow))]
    private async Task FollowAsync()
    {
        var result = await App.AppViewModel.MakoClient.PostFollowUserAsync(Id, PrivacyPolicy.Public);
        if (result)
        {
            UserDetail.UserEntity.IsFollowed = true;
            IsFollowed = true;
        }
    }

    [RelayCommand(CanExecute = nameof(CanFollow))]
    private async Task FollowPrivatelyAsync()
    {
        var result = await App.AppViewModel.MakoClient.PostFollowUserAsync(Id, PrivacyPolicy.Private);
        if (result)
        {
            UserDetail.UserEntity.IsFollowed = true;
            IsFollowed = true;
        }
    }

    [RelayCommand(CanExecute = nameof(CanFollow))]
    private async Task UnfollowAsync()
    {
        var result = await App.AppViewModel.MakoClient.RemoveFollowUserAsync(Id);
        if (result)
        {
            UserDetail.UserEntity.IsFollowed = false;
            IsFollowed = false;
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        AvatarSource?.Dispose();
        if (!ReferenceEquals(AvatarSource, BackgroundSource))
            BackgroundSource?.Dispose();
    }
}
