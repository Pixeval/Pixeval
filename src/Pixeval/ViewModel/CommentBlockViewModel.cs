using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using Pixeval.CoreApi.Model;
using Pixeval.CoreApi.Net;
using Pixeval.Utilities;
using Pixeval.Misc;
using Pixeval.Util.IO;
using Pixeval.Util.UI;

namespace Pixeval.ViewModel
{
    public class CommentBlockViewModel
    {
        public const string AddCommentUrlSegment = "/v1/illust/comment/add";

        public CommentBlockViewModel(Comment comment, string illustrationId)
        {
            Comment = comment;
            IllustrationId = illustrationId;
        }

        public string IllustrationId { get; }

        public Comment Comment { get; }

        public bool HasReplies => Comment.HasReplies;

        public bool IsStamp => Comment.CommentStamp is not null;

        public string? StampSource => Comment.CommentStamp?.StampUrl;

        public DateTimeOffset PostDate => Comment.Date;

        public string Poster => Comment.CommentPoster?.Name ?? string.Empty;

        public string PosterId => Comment.CommentPoster?.Id.ToString()!;

        public string CommentContent => Comment.CommentContent ?? string.Empty;

        public string CommentId => Comment.Id.ToString();

        public ObservableCollection<CommentBlockViewModel>? Replies { get; private set; }

        public async Task LoadRepliesAsync()
        {
            Replies = (await App.AppViewModel.MakoClient.IllustrationCommentReplies(CommentId)
                    .Select(c => new CommentBlockViewModel(c, IllustrationId))
                    .ToListAsync())
                .ToObservableCollection();
        }

        public async Task<ImageSource> GetAvatarSource()
        {
            return (await App.AppViewModel.MakoClient.DownloadBitmapImageResultAsync(Comment.CommentPoster!.ProfileImageUrls!.Medium!)
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