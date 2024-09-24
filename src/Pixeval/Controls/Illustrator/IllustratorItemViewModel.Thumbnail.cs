using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;
using Pixeval.AppManagement;
using Pixeval.CoreApi.Model;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Util.UI;
using Pixeval.Utilities;

namespace Pixeval.Controls;

public partial class IllustratorItemViewModel
{
    [ObservableProperty]
    private ImageSource? _avatarSource;

    [ObservableProperty]
    private SolidColorBrush _avatarBorderBrush = _defaultAvatarBorderColorBrush;

    public List<ImageSource> BannerSources { get; } = new(3);

    /// <summary>
    /// Dominant color of the "No Image" image
    /// </summary>
    private static readonly SolidColorBrush _defaultAvatarBorderColorBrush = new(UiHelper.ParseHexColor("#D6DEE5"));

    public async Task LoadAvatarAsync()
    {
        var result = await App.AppViewModel.MakoClient.DownloadMemoryStreamAsync(AvatarUrl);
        var stream = result is Result<Stream>.Success { Value: var avatar }
            ? avatar
            : AppInfo.GetPixivNoProfileStream();
        var dominantColor = await UiHelper.GetDominantColorAsync(stream, false);
        AvatarBorderBrush = new SolidColorBrush(dominantColor);
        stream.Position = 0;
        var source = await stream.GetBitmapImageAsync(true, 100, AvatarUrl);
        AvatarSource = source;

        await LoadBannerSourceAsync();
    }

    public override void Dispose()
    {
        AvatarSource = null;
        // foreach (var softwareBitmapSource in BannerSources)
        //     softwareBitmapSource.Dispose();
        BannerSources.Clear();
    }

    private async Task LoadBannerSourceAsync()
    {
        foreach (var entry in Entry.Illustrations)
            if (!await AddBannerSource(entry))
                break;

        foreach (var entry in Entry.Novels)
            if (!await AddBannerSource(entry))
                break;

        OnPropertyChanged(nameof(BannerSources));

        return;
        async Task<bool> AddBannerSource(IWorkEntry viewModel)
        {
            // 一般只会取 ==
            if (BannerSources.Count >= 3)
                return false;

            if (await App.AppViewModel.MakoClient.DownloadMemoryStreamAsync(viewModel.GetThumbnailUrl()) is not
                Result<Stream>.Success(var stream))
                return true;

            var bitmapSource = await stream.GetBitmapImageAsync(true, url: viewModel.GetThumbnailUrl());
            BannerSources.Add(bitmapSource);
            return true;
        }
    }
}
