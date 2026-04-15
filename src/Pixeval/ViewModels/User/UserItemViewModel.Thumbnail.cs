// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Linq;
using System.Threading.Tasks;
using AnimatedControls.Avalonia;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Mako.Model;
using Pixeval.Utilities;
using Pixeval.Utilities.IO.Caching;

namespace Pixeval.ViewModels;

public partial class UserItemViewModel
{
    [ObservableProperty]
    public partial IAnimatedBitmap? Avatar { get; private set; }

    [ObservableProperty]
    public partial Bitmap? Banner0 { get; private set; }

    [ObservableProperty]
    public partial Bitmap? Banner1 { get; private set; }

    [ObservableProperty]
    public partial Bitmap? Banner2 { get; private set; }

    private bool _loaded;

    public async Task LoadAvatarAsync()
    {
        if (_loaded)
            return;
        _loaded = true;
        var stream = await CacheHelper.GetStreamFromCacheAsync(AvatarUrl);
        stream.Position = 0;
        Avatar = await CacheHelper.GetAnimatedBitmapFromCacheAsync(AvatarUrl);

        await LoadBannerSourceAsync();
    }

    public void Dispose()
    {
        var avatar = Avatar;
        Avatar = null;
        avatar?.Dispose();
        var banner0 = Banner0;
        Banner0 = null;
        banner0?.Dispose();
        var banner1 = Banner1;
        Banner1 = null;
        banner1?.Dispose();
        var banner2 = Banner2;
        Banner2 = null;
        banner2?.Dispose();
    }

    private async Task LoadBannerSourceAsync()
    {
        var workEntries = Entry.Illustrations.Concat<IWorkEntry>(Entry.Novels).ToArray();
        if (workEntries.ElementAtOrDefault(0) is { } t0)
        {
            Banner0 = await CacheHelper.GetBitmapFromCacheAsync(t0.GetThumbnailUrl());
            if (workEntries.ElementAtOrDefault(1) is { } t1)
            {
                Banner1 = await CacheHelper.GetBitmapFromCacheAsync(t1.GetThumbnailUrl());
                if (workEntries.ElementAtOrDefault(2) is { } t2)
                    Banner2 = await CacheHelper.GetBitmapFromCacheAsync(t2.GetThumbnailUrl());
            }
        }
    }
}
