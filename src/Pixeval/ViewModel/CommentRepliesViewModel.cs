using System;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.CoreApi.Net.Response;

namespace Pixeval.ViewModel
{
    public class CommentRepliesViewModel : CommentViewModel
    {
        public CommentRepliesViewModel(IllustrationCommentsResponse.Comment comment)
        {
            Comment = comment;
        }

        public IllustrationCommentsResponse.Comment Comment { get; }

        public override string Poster => Comment.User?.Name ?? string.Empty;

        public override string CommentContent => Comment.CommentContent ?? string.Empty;

        public override DateTimeOffset PostDate => Comment.Date;
    }
}