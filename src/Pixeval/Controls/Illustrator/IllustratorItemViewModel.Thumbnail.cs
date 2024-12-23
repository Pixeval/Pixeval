using System.Collections.Generic;
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
    public partial ImageSource? AvatarSource { get; set; }

    [ObservableProperty]
    public partial SolidColorBrush AvatarBorderBrush { get; set; } = _defaultAvatarBorderColorBrush;

    public List<ImageSource> BannerSources { get; } = new(3);

    /// <summary>
    /// Dominant color of the "No Image" image
    /// </summary>
    private static readonly SolidColorBrush _defaultAvatarBorderColorBrush = new(UiHelper.ParseHexColor("#D6DEE5"));

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
        BannerSources.Clear();
    }

    private async Task LoadBannerSourceAsync()
    {
        foreach (var entry in Entry.Illustrations.Concat<IWorkEntry>(Entry.Novels).Take(3))
            await AddBannerSource(entry);

        OnPropertyChanged(nameof(BannerSources));

        return;
        async Task AddBannerSource(IWorkEntry viewModel)
        {
            BannerSources.Add(await App.AppViewModel.AppServiceProvider.GetRequiredService<MemoryCache>().GetSourceFromMemoryCacheAsync(viewModel.GetThumbnailUrl()));
        }
    }
}
