using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.CoreApi.Net;
using Pixeval.Misc;
using Pixeval.UserControls;
using Pixeval.Util.IO;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using Pixeval.ViewModel;

namespace Pixeval.Pages.IllustrationViewer
{
    public sealed partial class PixivReplyEmojiListPage
    {
        public static ObservableCollection<PixivReplyEmojiViewModel> EmojiList = new();

        private PixivReplyBar? _replyBar;

        static PixivReplyEmojiListPage()
        {
            LoadEmojis();
        }

        public PixivReplyEmojiListPage()
        {
            InitializeComponent();
        }

        public override void Prepare(NavigationEventArgs e)
        {
             _replyBar = (PixivReplyBar) e.Parameter;
        }

        private static async void LoadEmojis()
        {
            using var semaphoreSlim = new SemaphoreSlim(1, 1);
            await semaphoreSlim.WaitAsync();
            if (!EmojiList.Any())
            {
                var results = await Task.WhenAll(Enum.GetValues<PixivReplyEmoji>()
                    .Select(async emoji => (emoji, await App.AppViewModel.MakoClient.GetMakoHttpClient(MakoApiKind.ImageApi).DownloadAsIRandomAccessStreamAsync(emoji.GetReplyEmojiDownloadUrl()))));
                var tasks = results.Where(r => r.Item2 is Result<IRandomAccessStream>.Success).Select(async r => new PixivReplyEmojiViewModel(r.emoji, ((Result<IRandomAccessStream>.Success) r.Item2).Value)
                {
                    ImageSource = await ((Result<IRandomAccessStream>.Success) r.Item2).Value.GetBitmapImageAsync(false)
                });
                EmojiList.AddRange(await Task.WhenAll(tasks));
            }
        }

        private void EmojiImage_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            var viewModel = sender.GetDataContext<PixivReplyEmojiViewModel>();
            _replyBar?.ReplyContentRichEditBox.Document.Selection.InsertImage(20, 20, 17, VerticalCharacterAlignment.Baseline, viewModel.EmojiEnumValue.GetReplyEmojiPlaceholderKey(), viewModel.ImageStream);
        }
    }
}
