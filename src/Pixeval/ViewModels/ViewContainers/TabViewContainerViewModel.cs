// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Threading.Tasks;
using AnimatedControls.Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mako;
using Mako.Model;
using Pixeval.Utilities;
using Pixeval.Utilities.IO.Caching;

namespace Pixeval.ViewModels;

public partial class TabViewContainerViewModel : ObservableObject
{
    public TabViewContainerViewModel()
    {
        App.AppViewModel.MakoClient.TokenRefreshed += OnTokenRefreshed;
    }

    private async void OnTokenRefreshed(MakoClient sender, TokenResponse? e)
    {
        User = e?.User;
        Avatar = e is null ? null : await CacheHelper.GetAnimatedBitmapFromCacheAsync(e.User.ProfileImageUrls.Px50X50);
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

    public Uri? Url => User?.Id is { } id ? MakoHelper.GenerateUserWebUri(id) : null;

    [ObservableProperty]
    public partial bool RestrictedModeIdle { get; private set; } = true;

    [ObservableProperty]
    public partial bool AiShowIdle { get; private set; } = true;

    [ObservableProperty]
    public partial bool RestrictedCache { get; private set; }

    [ObservableProperty]
    public partial bool AiShowCache { get; private set; }

    [RelayCommand(CanExecute = nameof(RestrictedModeIdle))]
    private Task ToggleRestrictedModeAsync() => ToggleRestrictedModeAsync(false);

    [RelayCommand(CanExecute = nameof(AiShowIdle))]
    private Task ToggleAiShowAsync() => ToggleAiShowAsync(false);

    private async Task ToggleRestrictedModeAsync(bool skipPost)
    {
        if (!RestrictedModeIdle)
            return;
        RestrictedModeIdle = false;
        try
        {
            if (!skipPost)
            {
                if (await App.AppViewModel.MakoClient.PostRestrictedModeSettingsAsync(!RestrictedCache))
                    RestrictedCache = !RestrictedCache;
            }
            else
                RestrictedCache = await App.AppViewModel.MakoClient.GetRestrictedModeSettingsAsync();
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
            if (!skipPost)
            {
                if (await App.AppViewModel.MakoClient.PostAiShowSettingsAsync(!AiShowCache))
                    AiShowCache = !AiShowCache;
            }
            else
                AiShowCache = await App.AppViewModel.MakoClient.GetAiShowSettingsAsync();
        }
        finally
        {
            AiShowIdle = true;
        }
    }
}
