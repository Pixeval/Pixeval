using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using Pixeval.CoreApi.Net;
using Pixeval.CoreApi.Net.Response;
using Pixeval.Utilities;
using Pixeval.Misc;
using Pixeval.Util;
using Pixeval.Util.Generic;
using Pixeval.Util.IO;
using Pixeval.Util.UI;

namespace Pixeval.ViewModel
{
    public class CommentBlockViewModel
    {
        public CommentBlockViewModel(IllustrationCommentsResponse.Comment comment)
        {
            Comment = comment;
        }

        public IllustrationCommentsResponse.Comment Comment { get; }

        public bool HasReplies => Comment.HasReplies;

        public bool IsStamp => Comment.Stamp is not null;

        public string? StampSource => Comment.Stamp?.StampUrl;

        public DateTimeOffset PostDate => Comment.Date;

        public string Poster => Comment.User?.Name ?? string.Empty;

        public string CommentContent => Comment.CommentContent ?? string.Empty;

        public ObservableCollection<CommentBlockViewModel>? Replies { get; private set; }

        public async Task LoadRepliesAsync()
        {
            Replies = (await App.AppViewModel.MakoClient.IllustrationCommentReplies(Comment.Id.ToString())
                    .Select(c => new CommentBlockViewModel(c))
                    .ToListAsync())
                .ToObservableCollection();
        }

        public async Task<ImageSource> GetAvatarSource()
        {
            return (await App.AppViewModel.MakoClient.DownloadBitmapImageResultAsync(Comment.User!.ProfileImageUrls!.Medium!)
                .GetOrElseAsync(await AppContext.GetPixivNoProfileImageAsync())!)!;
        }

        public async Task<Paragraph> GetReplyContentParagraphAsync()
        {
            var paragraph = new Paragraph();
            foreach (var replyContentToken in ReplyEmojiHelper.EnumerateTokens(CommentContent))
            {
                switch (replyContentToken)
                {
                    case ReplyContentToken.TextToken(var content):
                        paragraph.Inlines.Add(new Run
                        {
                            Text = content
                        });
                        break;
                    case ReplyContentToken.EmojiToken(var emoji) when await App.AppViewModel.MakoClient.GetMakoHttpClient(MakoApiKind.ImageApi).DownloadAsIRandomAccessStreamAsync(emoji.GetReplyEmojiDownloadUrl()) is Result<IRandomAccessStream>.Success(var emojiSource):
                        paragraph.Inlines.Add(new InlineUIContainer
                        {
                            Child = new Image
                            {
                                VerticalAlignment = VerticalAlignment.Bottom,
                                Source = await emojiSource.GetBitmapImageAsync(true),
                                Width = 14,
                                Height = 14
                            }
                        });

                        break;
                }
            }

            return paragraph;
        }
    }
}