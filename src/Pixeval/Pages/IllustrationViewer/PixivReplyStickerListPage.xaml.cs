using System;
using Microsoft.UI.Xaml.Media;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.CoreApi.Net;
using Pixeval.UserControls;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using Pixeval.ViewModel;

namespace Pixeval.Pages.IllustrationViewer
{
    public sealed partial class PixivReplyStickerListPage
    {
        public static readonly ObservableCollection<PixivReplyStickerViewModel> Stickers = new();

        private EventHandler<StickerTappedEventArgs>? _replyBarStickerTappedEventHandler;

        static PixivReplyStickerListPage()
        {
            LoadStickers();
        }

        public PixivReplyStickerListPage()
        {
            InitializeComponent();
        }

        public override void Prepare(NavigationEventArgs e)
        {
            _replyBarStickerTappedEventHandler = (((Guid, EventHandler<StickerTappedEventArgs>)) e.Parameter).Item2;
        }

        private static async void LoadStickers()
        {
            using var semaphoreSlim = new SemaphoreSlim(1, 1);
            await semaphoreSlim.WaitAsync();
            if (!Stickers.Any())
            {
                var results = await Task.WhenAll(MakoHelper.StickerIds
                    .Select(async id => (id, await App.AppViewModel.MakoClient.GetMakoHttpClient(MakoApiKind.ImageApi).DownloadAsIRandomAccessStreamAsync(MakoHelper.GenerateStickerDownloadUrl(id)))));
                var tasks = results.Where(r => r.Item2 is Result<IRandomAccessStream>.Success)
                    .Select(async r => new PixivReplyStickerViewModel(r.id, ((Result<IRandomAccessStream>.Success) r.Item2).Value)
                    {
                        ImageSource = await ((Result<IRandomAccessStream>.Success) r.Item2).Value.GetBitmapImageAsync(false)
                    });
                Stickers.AddRange(await Task.WhenAll(tasks));
            }
        }

        private void StickerImage_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            _replyBarStickerTappedEventHandler?.Invoke(sender, new StickerTappedEventArgs(e, sender.GetDataContext<PixivReplyStickerViewModel>()));
        }
    }
}
