#region Copyright (c) $SOLUTION$/$PROJECT$

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/IllustratorIllustrationsOverviewViewModel.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.Options;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using Pixeval.CoreApi.Model;

namespace Pixeval.Controls;

public partial class IllustratorIllustrationsOverviewViewModel : ObservableObject, IDisposable
{
    // Dominant color of the "No Image" image
    private static readonly SolidColorBrush _defaultAvatarBorderColorBrush =
        new(UiHelper.ParseHexColor("#D6DEE5"));

    [ObservableProperty]
    private Brush? _avatarBorderBrush;

    private const ThumbnailUrlOption Option = ThumbnailUrlOption.SquareMedium;

    public IllustratorIllustrationsOverviewViewModel(IEnumerable<long>? ids)
    {
        GetAvatarBorderBrush = false;
        _ = SetBannerSourceFromIdsAsync(ids);
    }

    public IllustratorIllustrationsOverviewViewModel(IEnumerable<Illustration>? illustrations)
    {
        GetAvatarBorderBrush = true;
        _ = SetBannerSourceFromIllustrationsAsync(illustrations);
    }

    private bool GetAvatarBorderBrush { get; }

    public List<SoftwareBitmapSource> BannerSources { get; } = new(3);

    public void Dispose()
    {
        foreach (var softwareBitmapSource in BannerSources)
            softwareBitmapSource.Dispose();
        BannerSources.Clear();
    }

    private async Task SetBannerSourceFromIdsAsync(IEnumerable<long>? ids)
    {
        var illustrations = new List<Illustration>();
        if (ids is not null)
            foreach (var id in ids)
            {
                var illustration = await App.AppViewModel.MakoClient.GetIllustrationFromIdAsync(id);
                illustrations.Add(illustration);
            }

        await SetBannerSourceFromIllustrationsAsync(illustrations);
    }

    private async Task SetBannerSourceFromIllustrationsAsync(IEnumerable<Illustration>? illustrations)
    {
        AvatarBorderBrush = null;
        Dispose();
        if (illustrations is not null)
            foreach (var illustration in illustrations)
            {
                if (illustration.GetThumbnailUrl(Option) is { } url)
                {
                    if (await App.AppViewModel.MakoClient.DownloadRandomAccessStreamAsync(url) is not
                        Result<IRandomAccessStream>.Success(var stream))
                        continue;
                    if (AvatarBorderBrush is null && GetAvatarBorderBrush)
                    {
                        var dominantColor = await UiHelper.GetDominantColorAsync(stream.AsStreamForRead(), false);
                        AvatarBorderBrush = new SolidColorBrush(dominantColor);
                    }

                    var bitmapSource = await stream.GetSoftwareBitmapSourceAsync(true);
                    BannerSources.Add(bitmapSource);

                    // 一般只会取 ==
                    if (BannerSources.Count >= 3)
                        break;
                }
            }

        OnPropertyChanged(nameof(BannerSources));

        if (AvatarBorderBrush is null && GetAvatarBorderBrush)
            AvatarBorderBrush = _defaultAvatarBorderColorBrush;
    }
}
