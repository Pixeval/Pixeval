using Microsoft.UI.Xaml.Media;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Pixeval.Util;
using Pixeval.Util.IO;
using Pixeval.Utilities;
using Pixeval.ViewModel;

namespace Pixeval.Pages.IllustrationViewer
{
    public sealed partial class PixivReplyStickerListPage
    {
        public static readonly ObservableCollection<PixivReplyStickerViewModel> Stickers = new();

        static PixivReplyStickerListPage()
        {
            LoadStickers();
        }

        public PixivReplyStickerListPage()
        {
            InitializeComponent();
        }

        private static async void LoadStickers()
        {
            using var semaphoreSlim = new SemaphoreSlim(1, 1);
            await semaphoreSlim.WaitAsync();
            if (!Stickers.Any())
            {
                var results = await Task.WhenAll(MakoHelper.StickerIds
                    .Select(async id => (id, await App.AppViewModel.MakoClient.DownloadBitmapImageResultAsync(MakoHelper.GenerateStickerDownloadUrl(id)))));
                Stickers.AddRange(results.Where(r => r.Item2 is Result<ImageSource>.Success).Select(r => new PixivReplyStickerViewModel(r.id, ((Result<ImageSource>.Success) r.Item2).Value)));
            }
        }
    }
}
