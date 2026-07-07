// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Threading.Tasks;
using AnimatedControls.Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mako;
using Mako.Model;
using Misaki;
using Pixeval.Utilities.IO.Caching;

namespace Pixeval.ViewModels;

public partial class TabViewContainerViewModel : ViewModelBase
{
    public TabViewContainerViewModel()
    {
        OnUserRefreshed(App.AppViewModel.MakoClient.Me);
        App.AppViewModel.MakoClient.TokenRefreshed += OnTokenRefreshed;
    }

    private void OnTokenRefreshed(MakoClient sender, TokenResponse? e)
    {
        OnUserRefreshed(e?.User);
    }

    private async void OnUserRefreshed(TokenUser? user)
    {
        User = user;
        Avatar = user is null ? null : await CacheHelper.GetAnimatedBitmapAsync(IPlatformInfo.Pixiv, user.ProfileImageUrls.Px50X50);
        // await ToggleRestrictedModeAsync(true);
        await ToggleAiShowAsync(true);
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IdText))]
    [NotifyPropertyChangedFor(nameof(Url))]
    public partial TokenUser? User { get; private set; }

    [ObservableProperty]
    public partial IAnimatedBitmap? Avatar { get; private set; }

    public string? IdText => User?.Id.ToString();

    public Uri? Url => User?.WebsiteUri;

    [ObservableProperty]
    public partial bool RestrictedModeIdle { get; private set; } = true;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ToggleAiShowCommand))]
    public partial bool AiShowIdle { get; private set; } = true;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ToggleRestrictedModeCommand))]
    public partial bool RestrictedCache { get; private set; }

    [ObservableProperty]
    public partial bool AiShowCache { get; private set; }

    [RelayCommand(CanExecute = nameof(AiShowIdle))]
    private Task ToggleAiShowAsync() => ToggleAiShowAsync(false);

    [RelayCommand(CanExecute = nameof(RestrictedModeIdle))]
    private Task ToggleRestrictedModeAsync() => ToggleRestrictedModeAsync(false);

    private async Task ToggleRestrictedModeAsync(bool skipPost)
    {
        if (!RestrictedModeIdle)
            return;
        RestrictedModeIdle = false;
        try
        {
            RestrictedCache = skipPost
                ? await App.AppViewModel.MakoClient.GetRestrictedModeSettingsAsync()
                : await App.AppViewModel.MakoClient.PostRestrictedModeSettingsAsync(!RestrictedCache);
        }
        finally
        {
            RestrictedModeIdle = true;
        }
    }

    private async Task ToggleAiShowAsync(bool skipPost)
    {
        if (!AiShowIdle)
            return;
        AiShowIdle = false;
        try
        {
            AiShowCache = skipPost
                ? await App.AppViewModel.MakoClient.GetAiShowSettingsAsync()
                : await App.AppViewModel.MakoClient.PostAiShowSettingsAsync(!AiShowCache);
        }
        finally
        {
            AiShowIdle = true;
        }
    }
}
