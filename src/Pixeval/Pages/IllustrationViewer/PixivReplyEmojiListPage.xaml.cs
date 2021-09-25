using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media;
using Pixeval.Misc;
using Pixeval.Util.IO;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using Pixeval.ViewModel;

namespace Pixeval.Pages.IllustrationViewer
{
    public sealed partial class PixivReplyEmojiListPage
    {
        public static ObservableCollection<PixivReplyEmojiViewModel> EmojiList = new();

        static PixivReplyEmojiListPage()
        {
            LoadEmojis();
        }

        public PixivReplyEmojiListPage()
        {
            InitializeComponent();
        }
        
        private static async void LoadEmojis()
        {
            using var semaphoreSlim = new SemaphoreSlim(1, 1);
            await semaphoreSlim.WaitAsync();
            if (!EmojiList.Any())
            {
                var results = await Task.WhenAll(Enum.GetValues<PixivReplyEmoji>()
                    .Select(async emoji => (emoji, await App.AppViewModel.MakoClient.DownloadBitmapImageResultAsync(emoji.GetReplyEmojiDownloadUrl()))));
                EmojiList.AddRange(results.Where(r => r.Item2 is Result<ImageSource>.Success).Select(r => new PixivReplyEmojiViewModel(r.emoji, ((Result<ImageSource>.Success) r.Item2).Value)));
            }
        }
    }
}
