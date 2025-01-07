// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Controls;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Util.IO.Caching;
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
                .Select(async id => new PixivReplyStickerViewModel(id,
                    await App.AppViewModel.AppServiceProvider.GetRequiredService<MemoryCache>()
                        .GetSourceFromMemoryCacheAsync(MakoHelper.GenerateStickerDownloadUrl(id), desiredWidth: 83))));
            Stickers.AddRange(results.WhereNotNull());
        }
    }

    private void StickerImage_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        _replyBarStickerClickEventHandler?.Invoke(sender, new StickerClickEventArgs(e, sender.To<FrameworkElement>().GetTag<PixivReplyStickerViewModel>()));
    }
}
