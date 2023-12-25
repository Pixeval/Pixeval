#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/PixivReplyEmojiListPage.xaml.cs
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
using Windows.Storage.Streams;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Controls;
using Pixeval.CoreApi.Net;
using Pixeval.Misc;
using Pixeval.Util.IO;
using Pixeval.Util.UI;
using Pixeval.Utilities;
using WinUI3Utilities;

namespace Pixeval.Pages.IllustrationViewer;

public sealed partial class PixivReplyEmojiListPage
{
    public static ObservableCollection<PixivReplyEmojiViewModel> EmojiList = [];

    private PixivReplyBar? _replyBar;

    static PixivReplyEmojiListPage()
    {
        LoadEmojis();
    }

    public PixivReplyEmojiListPage()
    {
        InitializeComponent();
    }

    public override void OnPageActivated(NavigationEventArgs e)
    {
        _replyBar = (PixivReplyBar)e.Parameter;
    }

    private static async void LoadEmojis()
    {
        using var semaphoreSlim = new SemaphoreSlim(1, 1);
        await semaphoreSlim.WaitAsync();
        if (!EmojiList.Any())
        {
            var results = await Task.WhenAll(Enum.GetValues<PixivReplyEmoji>()
                .Select(async emoji => (emoji, await App.AppViewModel.MakoClient.DownloadRandomAccessStreamAsync(emoji.GetReplyEmojiDownloadUrl()))));
            var tasks = results.Where(r => r.Item2 is Result<IRandomAccessStream>.Success).Select(async r => new PixivReplyEmojiViewModel(r.emoji, ((Result<IRandomAccessStream>.Success)r.Item2).Value)
            {
                // We don't dispose of the image here because it will be used when inserting emoji to the rich edit box.
                ImageSource = await ((Result<IRandomAccessStream>.Success)r.Item2).Value.GetBitmapImageAsync(false, 35)
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
