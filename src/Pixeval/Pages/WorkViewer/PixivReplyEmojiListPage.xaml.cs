// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Controls;
using Pixeval.Util.IO;
using Pixeval.Util.IO.Caching;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using WinUI3Utilities;

namespace Pixeval.Pages;

public sealed partial class PixivReplyEmojiListPage
{
    public static readonly ObservableCollection<PixivReplyEmojiViewModel> EmojiList = [];

    private PixivReplyBar? _replyBar;

    static PixivReplyEmojiListPage() => LoadEmojis();

    public PixivReplyEmojiListPage() => InitializeComponent();

    public override void OnPageActivated(NavigationEventArgs e, object? parameter)
    {
        _replyBar = (PixivReplyBar)e.Parameter;
    }

    private static async void LoadEmojis()
    {
        using var semaphoreSlim = new SemaphoreSlim(1, 1);
        await semaphoreSlim.WaitAsync();
        if (EmojiList.Count is 0)
        {
            var memoryCache = App.AppViewModel.AppServiceProvider.GetRequiredService<MemoryCache>();
            var tasks = await Task.WhenAll(Enum.GetValues<PixivReplyEmoji>()
                .Select(async emoji => new PixivReplyEmojiViewModel(
                    emoji, 
                    await memoryCache.GetStreamFromMemoryCacheAsync(emoji.GetReplyEmojiDownloadUrl()), 
                    await memoryCache.GetSourceFromMemoryCacheAsync(emoji.GetReplyEmojiDownloadUrl()))));
            EmojiList.AddRange(tasks);
        }
    }

    private void EmojiImage_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        var viewModel = sender.To<FrameworkElement>().GetTag<PixivReplyEmojiViewModel>();
        _replyBar?.ReplyContentRichEditBox.Document.Selection.InsertImage(20, 20, 17, VerticalCharacterAlignment.Baseline, viewModel.EmojiEnumValue.GetReplyEmojiPlaceholderKey(), viewModel.ImageStream.AsRandomAccessStream());
    }
}
