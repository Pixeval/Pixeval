#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2024 Pixeval/FeedItemCondensedViewModel.cs
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

using Microsoft.UI.Xaml.Media;
using Pixeval.CoreApi.Model;
using Pixeval.Util.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Microsoft.UI;
using Pixeval.AppManagement;
using WinUI3Utilities;

namespace Pixeval.Pages.Capability.Feeds;

public class FeedItemCondensedViewModel(List<Feed?> entries) : AbstractFeedItemViewModel(new IFeedEntry.CondensedFeedEntry(entries))
{
    public override void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public override Uri AppUri => ThrowHelper.NotSupported<Uri>("AppUri is not supported for condensed feeds");

    public override Uri WebUri => ThrowHelper.NotSupported<Uri>("WebUri is not supported for condensed feeds");

    public override Uri PixEzUri => ThrowHelper.NotSupported<Uri>("PixEzUri is not supported for condensed feeds");

    private ImageSource? _userAvatar;

    // It's impossible to use [ObservableProperty] here, for that generated properties lack the `override` modifier
    // same for the ItemBackground property
    public override ImageSource UserAvatar
    {
        get => _userAvatar!;
        protected set => SetProperty(ref _userAvatar, value);
    }

    private SolidColorBrush _itemBackground = new(Colors.Transparent);

    public override SolidColorBrush ItemBackground
    {
        get => _itemBackground;
        set => SetProperty(ref _itemBackground, value);
    }

    public override string PostUsername => entries[0]?.PostUsername ?? string.Empty;

    public override string PostDateFormatted => $"{FormatDate(entries[^1]?.PostDate ?? default)} ~ {FormatDate(entries[0]?.PostDate ?? default)}";

    private static string FormatDate(DateTimeOffset postDate)
    {
        return (DateTime.Now - postDate) < TimeSpan.FromDays(1)
            ? postDate.ToString("hh:mm tt")
            : postDate.ToString("M");
    }

    public override async Task LoadAsync()
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
#pragma warning disable MVVMTK0034
        if (_userAvatar is not null)
#pragma warning restore MVVMTK0034
            return;

        if (entries[0]?.PostUserThumbnail is { Length: > 0 } url)
        {
            var image = (await App.AppViewModel.MakoClient.DownloadBitmapImageWithDesiredSizeAsync(url, 35)).UnwrapOrElse(await AppInfo.ImageNotAvailable)!;
            UserAvatar = image;
        }
        else
        {
            UserAvatar = await AppInfo.ImageNotAvailable;
        }
    }
}
