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
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using Pixeval.CoreApi.Model;

namespace Pixeval.Controls;

public partial class IllustratorIllustrationsOverviewViewModel : ObservableObject, IDisposable
{
    /// <summary>
    /// Dominant color of the "No Image" image
    /// </summary>
    private static readonly SolidColorBrush _defaultAvatarBorderColorBrush = new(UiHelper.ParseHexColor("#D6DEE5"));

    [ObservableProperty]
    private Brush? _avatarBorderBrush;

    public IllustratorIllustrationsOverviewViewModel(IEnumerable<long>? ids)
    {
        _getAvatarBorderBrush = false;
        LoadBannerSource = () => LoadBannerSourceFromIdsAsync(ids);
    }

    public IllustratorIllustrationsOverviewViewModel(IEnumerable<Illustration>? illustrations)
    {
        _getAvatarBorderBrush = true;
        LoadBannerSource = () => LoadBannerSourceFromIllustrationsAsync(illustrations);
    }

    public Func<Task> LoadBannerSource { get; }

    private readonly bool _getAvatarBorderBrush;

    public List<SoftwareBitmapSource> BannerSources { get; } = new(3);

    public void Dispose()
    {
        foreach (var softwareBitmapSource in BannerSources)
            softwareBitmapSource.Dispose();
        BannerSources.Clear();
    }

    private async Task LoadBannerSourceFromIdsAsync(IEnumerable<long>? ids)
    {
        var illustrations = new List<Illustration>();
        if (ids is not null)
            foreach (var id in ids)
            {
                var illustration = await App.AppViewModel.MakoClient.GetIllustrationFromIdAsync(id);
                illustrations.Add(illustration);
            }

        await LoadBannerSourceFromIllustrationsAsync(illustrations);
    }

    private async Task LoadBannerSourceFromIllustrationsAsync(IEnumerable<Illustration>? illustrations)
    {
        AvatarBorderBrush = null;
        Dispose();
        if (illustrations is not null)
            foreach (var illustration in illustrations)
            {
                if (illustration.GetThumbnailUrl() is { } url)
                {
                    if (await App.AppViewModel.MakoClient.DownloadStreamAsync(url) is not
                        Result<Stream>.Success(var stream))
                        continue;
                    if (AvatarBorderBrush is null && _getAvatarBorderBrush)
                    {
                        var dominantColor = await UiHelper.GetDominantColorAsync(stream, false);
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

        if (AvatarBorderBrush is null && _getAvatarBorderBrush)
            AvatarBorderBrush = _defaultAvatarBorderColorBrush;
    }
}
