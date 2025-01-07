using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Media;
using Pixeval.CoreApi.Model;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Util.IO.Caching;
using Pixeval.Util.UI;

namespace Pixeval.Controls;

public partial class IllustratorItemViewModel
{
    [ObservableProperty]
    public partial ImageSource? AvatarSource { get; private set; }

    [ObservableProperty]
    public partial SolidColorBrush AvatarBorderBrush { get; set; } = _DefaultAvatarBorderColorBrush;

    [ObservableProperty]
    public partial ImageSource? BannerSource0 { get; private set; }

    [ObservableProperty]
    public partial ImageSource? BannerSource1 { get; private set; }

    [ObservableProperty]
    public partial ImageSource? BannerSource2 { get; private set; }

    /// <summary>
    /// Dominant color of the "No Image" image
    /// </summary>
    private static readonly SolidColorBrush _DefaultAvatarBorderColorBrush = new(UiHelper.ParseHexColor("#D6DEE5"));

    public async Task LoadAvatarAsync()
    {
        var memoryCache = App.AppViewModel.AppServiceProvider.GetRequiredService<MemoryCache>();
        var stream = await memoryCache.GetStreamFromMemoryCacheAsync(AvatarUrl);
        var dominantColor = await UiHelper.GetDominantColorAsync(stream, false);
        AvatarBorderBrush = new SolidColorBrush(dominantColor);
        stream.Position = 0;
        AvatarSource = await memoryCache.GetSourceFromMemoryCacheAsync(AvatarUrl, desiredWidth: 100);

        await LoadBannerSourceAsync();
    }

    public override void Dispose()
    {
        AvatarSource = null;
    }

    private async Task LoadBannerSourceAsync()
    {
        var getSourceFromMemoryCacheAsync = App.AppViewModel.AppServiceProvider.GetRequiredService<MemoryCache>().GetSourceFromMemoryCacheAsync;
        if (Entry.Illustrations.Concat<IWorkEntry>(Entry.Novels).ElementAtOrDefault(0) is { } t0)
        {
            BannerSource0 = await getSourceFromMemoryCacheAsync(t0.GetThumbnailUrl());
            if (Entry.Illustrations.Concat<IWorkEntry>(Entry.Novels).ElementAtOrDefault(1) is { } t1)
            {
                BannerSource1 = await getSourceFromMemoryCacheAsync(t1.GetThumbnailUrl());
                if (Entry.Illustrations.Concat<IWorkEntry>(Entry.Novels).ElementAtOrDefault(0) is { } t2)
                    BannerSource2 = await getSourceFromMemoryCacheAsync(t2.GetThumbnailUrl());
            }
        }
    }
}
