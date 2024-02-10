#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/MakoHelper.cs
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
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.WinUI.Collections;
using Microsoft.UI;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Pixeval.Controls;
using Pixeval.Controls.MarkupExtensions;
using Pixeval.CoreApi.Global.Enum;
using Pixeval.CoreApi.Model;
using Pixeval.Misc;
using Pixeval.Options;
using Pixeval.Util.Generic;
using WinUI3Utilities;
using AppContext = Pixeval.AppManagement.AppContext;

namespace Pixeval.Util;

public static class MakoHelper
{
    public static IReadOnlyList<int> StickerIds =
    [
        .. Enumerable.Range(301, 10),
        .. Enumerable.Range(401, 10),
        .. Enumerable.Range(201, 10),
        .. Enumerable.Range(101, 10)
    ];

    public static IllustrationSortOptionWrapper GetAppSettingDefaultSortOptionWrapper()
    {
        return LocalizedBoxHelper.Of<IllustrationSortOption, IllustrationSortOptionWrapper>(App.AppViewModel.AppSetting.DefaultSortOption);
    }

    public static string GetThumbnailUrl(this Illustration illustration, ThumbnailUrlOption option)
    {
        return option switch
        {
            ThumbnailUrlOption.Large => illustration.ImageUrls.Large,
            ThumbnailUrlOption.Medium => illustration.ImageUrls.Medium,
            ThumbnailUrlOption.SquareMedium => illustration.ImageUrls.SquareMedium,
            _ => ThrowHelper.ArgumentOutOfRange<ThumbnailUrlOption, string?>(option)
        };
    }

    public static Uri GenerateIllustrationWebUri(long id)
    {
        return new Uri($"https://www.pixiv.net/artworks/{id}");
    }

    public static Uri GenerateIllustrationPixEzUri(long id)
    {
        return new Uri($"pixez://www.pixiv.net/artworks/{id}");
    }

    public static Uri GenerateIllustrationAppUri(long id)
    {
        return new Uri($"{AppContext.AppProtocol}://illust/{id}");
    }

    public static Uri GenerateIllustratorWebUri(long id)
    {
        return new Uri($"https://www.pixiv.net/users/{id}");
    }

    public static Uri GenerateIllustratorPixEzUri(long id)
    {
        return new Uri($"pixez://www.pixiv.net/users/{id}");
    }

    public static Uri GenerateIllustratorAppUri(long id)
    {
        return new Uri($"{AppContext.AppProtocol}://user/{id}");
    }

    public static async Task<string> GetIllustrationThumbnailCacheKeyAsync(this IllustrationItemViewModel illustration, ThumbnailUrlOption thumbnailUrlOption = ThumbnailUrlOption.Medium)
    {
        return $"thumbnail-{thumbnailUrlOption}-{await illustration.GetOriginalSourceUrlAsync()}";
    }

    public static async Task<string> GetIllustrationOriginalImageCacheKeyAsync(this IllustrationItemViewModel illustration)
    {
        return $"original-{await illustration.GetOriginalSourceUrlAsync()}";
    }

    public static SortDescription? GetSortDescriptionForIllustration(IllustrationSortOption sortOption)
    {
        return sortOption switch
        {
            IllustrationSortOption.PopularityDescending => new(SortDirection.Descending, IllustrationBookmarkComparer.Instance),
            IllustrationSortOption.PublishDateAscending => new(SortDirection.Ascending, IllustrationViewModelPublishDateComparer.Instance),
            IllustrationSortOption.PublishDateDescending => new(SortDirection.Descending, IllustrationViewModelPublishDateComparer.Instance),
            IllustrationSortOption.DoNotSort => null,
            _ => ThrowHelper.ArgumentOutOfRange<IllustrationSortOption, SortDescription?>(sortOption)
        };
    }

    public static string GenerateStickerDownloadUrl(int id)
    {
        return $"https://s.pximg.net/common/images/stamp/generated-stamps/{id}_s.jpg";
    }

    public static bool SetFollow(long id, bool isFollowed, bool privately = false)
    {
        _ = isFollowed
            ? App.AppViewModel.MakoClient.PostFollowUserAsync(id, privately ? PrivacyPolicy.Private : PrivacyPolicy.Public)
            : App.AppViewModel.MakoClient.RemoveFollowUserAsync(id);
        return isFollowed;
    }
}
