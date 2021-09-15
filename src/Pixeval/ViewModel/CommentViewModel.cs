using System;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.CoreApi.Net;
using Pixeval.Misc;
using Pixeval.Util.IO;
using Pixeval.Util.UI;

namespace Pixeval.ViewModel
{
    public abstract class CommentViewModel : ObservableObject
    {
        public abstract string Poster { get; }

        public abstract string CommentContent { get; }

        public abstract SoftwareBitmapSource? AvatarSource { get; set; }

        public abstract DateTimeOffset PostDate { get; }

        public async Task<Paragraph> GetReplyContentParagraphAsync()
        {
            var paragraph = new Paragraph();
            foreach (var replyContentToken in ReplyEmojiHelper.EnumerateTokens(CommentContent))
            {
                switch (replyContentToken)
                {
                    case ReplyContentToken.TextToken (var content):
                        paragraph.Inlines.Add(new Run
                        {
                            Text = content
                        });
                        break;
                    case ReplyContentToken.EmojiToken (var emoji):
                        var emojiSource = (await App.MakoClient.GetMakoHttpClient(MakoApiKind.ImageApi)
                                .DownloadAsIRandomAccessStreamAsync(emoji.GetReplyEmojiDownloadUrl()))
                            .GetOrElse(null);
                        if (emojiSource is not null)
                        {
                            paragraph.Inlines.Add(new InlineUIContainer
                            {
                                Child = new Image
                                {
                                    Source = await emojiSource.GetBitmapImageAsync(true),
                                    Width = 16,
                                    Height = 16
                                }
                            });
                        }
                        break;
                }
            }

            return paragraph;
        }
    }
}