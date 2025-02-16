// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;
using Pixeval.CoreApi.Model;
using Pixeval.Util;
using Pixeval.Util.IO.Caching;
using Pixeval.Util.UI;

namespace Pixeval.Controls;

public partial class IllustratorItemViewModel
{
    [ObservableProperty]
    public partial ImageSource? AvatarSource { get; private set; }

    [ObservableProperty]
    public partial SolidColorBrush AvatarBorderBrush { get; private set; } = _DefaultAvatarBorderColorBrush;

    [ObservableProperty]
    public partial ImageSource? BannerSource0 { get; private set; }

    [ObservableProperty]
    public partial ImageSource? BannerSource1 { get; private set; }

    [ObservableProperty]
    public partial ImageSource? BannerSource2 { get; private set; }

    private bool _loaded;

    /// <summary>
    /// Dominant color of the "No Image" image
    /// </summary>
    private static readonly SolidColorBrush _DefaultAvatarBorderColorBrush = new(UiHelper.ParseHexColor("#D6DEE5"));

    public async Task LoadAvatarAsync()
    {
        if (_loaded)
            return;
        _loaded = true;
        var stream = await CacheHelper.GetStreamFromCacheAsync(AvatarUrl);
        var dominantColor = await UiHelper.GetDominantColorAsync(stream, false);
        AvatarBorderBrush = new SolidColorBrush(dominantColor);
        stream.Position = 0;
        AvatarSource = await CacheHelper.GetSourceFromCacheAsync(AvatarUrl, desiredWidth: 100);

        await LoadBannerSourceAsync();
    }

    public override void Dispose()
    {
        AvatarSource = null;
        BannerSource0 = null;
        BannerSource1 = null;
        BannerSource2 = null;
    }

    private async Task LoadBannerSourceAsync()
    {
        var workEntries = Entry.Illustrations.Concat<IWorkEntry>(Entry.Novels).ToArray();
        if (workEntries.ElementAtOrDefault(0) is { } t0)
        {
            BannerSource0 = await CacheHelper.GetSourceFromCacheAsync(t0.GetThumbnailUrl());
            if (workEntries.ElementAtOrDefault(1) is { } t1)
            {
                BannerSource1 = await CacheHelper.GetSourceFromCacheAsync(t1.GetThumbnailUrl());
                if (workEntries.ElementAtOrDefault(2) is { } t2)
                    BannerSource2 = await CacheHelper.GetSourceFromCacheAsync(t2.GetThumbnailUrl());
            }
        }
    }
}
