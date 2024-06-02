#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/PixivReplyStickerListPage.xaml.cs
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
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Controls;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Utilities;
using WinUI3Utilities;

namespace Pixeval.Pages;

public sealed partial class PixivReplyStickerListPage
{
    public static readonly ObservableCollection<PixivReplyStickerViewModel> Stickers = [];

    private EventHandler<StickerClickEventArgs>? _replyBarStickerClickEventHandler;

    static PixivReplyStickerListPage() => LoadStickers();

    public PixivReplyStickerListPage() => InitializeComponent();

    public override void OnPageActivated(NavigationEventArgs e)
    {
        _replyBarStickerClickEventHandler = (((Guid, EventHandler<StickerClickEventArgs>))e.Parameter).Item2;
    }

    /// <summary>
    /// Load stickers once for all
    /// </summary>
    private static async void LoadStickers()
    {
        using var semaphoreSlim = new SemaphoreSlim(1, 1);
        await semaphoreSlim.WaitAsync();
        if (Stickers.Count is 0)
        {
            var results = await Task.WhenAll(MakoHelper.StickerIds
                .Select(async id => await App.AppViewModel.MakoClient.DownloadBitmapImageAsync(
                    MakoHelper.GenerateStickerDownloadUrl(id), 83) is Result<ImageSource>.Success { Value: { } result } ? new PixivReplyStickerViewModel(id, result) : null));
            Stickers.AddRange(results.WhereNotNull());
        }
    }

    private void StickerImage_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        _replyBarStickerClickEventHandler?.Invoke(sender, new StickerClickEventArgs(e, sender.To<FrameworkElement>().GetTag<PixivReplyStickerViewModel>()));
    }
}
