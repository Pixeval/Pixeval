#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/PixivReplyStickerListPage.xaml.cs
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
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.CoreApi.Net;
using Pixeval.Controls;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Utilities;
using Windows.Storage.Streams;
using WinUI3Utilities;

namespace Pixeval.Pages.IllustrationViewer;

public sealed partial class PixivReplyStickerListPage
{
    public static readonly ObservableCollection<PixivReplyStickerViewModel> Stickers = [];

    private EventHandler<StickerTappedEventArgs>? _replyBarStickerTappedEventHandler;

    static PixivReplyStickerListPage()
    {
        LoadStickers();
    }

    public PixivReplyStickerListPage()
    {
        InitializeComponent();
    }

    public override void OnPageActivated(NavigationEventArgs e)
    {
        _replyBarStickerTappedEventHandler = (((Guid, EventHandler<StickerTappedEventArgs>))e.Parameter).Item2;
    }

    /// <summary>
    /// Load stickers once for all
    /// </summary>
    private static async void LoadStickers()
    {
        using var semaphoreSlim = new SemaphoreSlim(1, 1);
        await semaphoreSlim.WaitAsync();
        if (!Stickers.Any())
        {
            var results = await Task.WhenAll(MakoHelper.StickerIds
                .Select(async id => (id, await App.AppViewModel.MakoClient.GetMakoHttpClient(MakoApiKind.ImageApi).DownloadAsIRandomAccessStreamAsync(MakoHelper.GenerateStickerDownloadUrl(id)))));
            var tasks = results.Where(r => r.Item2 is Result<IRandomAccessStream>.Success)
                .Select(r => (r.id, (Result<IRandomAccessStream>.Success)r.Item2))
                .Select(async r => new PixivReplyStickerViewModel(r.id, await r.Item2.Value.GetBitmapImageAsync(true, 83)));
            Stickers.AddRange(await Task.WhenAll(tasks));
        }
    }

    private void StickerImage_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        _replyBarStickerTappedEventHandler?.Invoke(sender, new StickerTappedEventArgs(e, sender.GetDataContext<PixivReplyStickerViewModel>()));
    }
}
