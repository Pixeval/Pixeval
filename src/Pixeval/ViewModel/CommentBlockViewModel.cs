using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media;
using Pixeval.CoreApi.Net.Response;
using Pixeval.Util;
using Pixeval.Util.Generic;
using Pixeval.Util.IO;

namespace Pixeval.ViewModel
{
    public class CommentBlockViewModel : CommentViewModel
    {
        public CommentBlockViewModel(IllustrationCommentsResponse.Comment comment)
        {
            Comment = comment;
        }

        public IllustrationCommentsResponse.Comment Comment { get; }

        public bool HasReplies => Comment.HasReplies;

        public bool IsStamp => Comment.Stamp is not null;

        public string? StampSource => Comment.Stamp?.StampUrl;

        public override DateTimeOffset PostDate => Comment.Date;

        public override string Poster => Comment.User?.Name ?? string.Empty;

        public override string CommentContent => Comment.CommentContent ?? string.Empty;

        public ObservableCollection<CommentRepliesViewModel>? Replies { get; private set; }

        public async Task LoadRepliesAsync()
        {
            Replies = (await App.MakoClient.IllustrationCommentReplies(Comment.Id.ToString())
                    .Select(c => new CommentRepliesViewModel(c))
                    .ToListAsync())
                .ToObservableCollection();
        }

        public async Task<ImageSource> GetAvatarSource()
        {
            return (await App.MakoClient.DownloadBitmapImageResultAsync(Comment.User!.ProfileImageUrls!.Medium!)
                .GetOrElseAsync(await AppContext.GetPixivNoProfileImageAsync())!)!;
        }
    }
}