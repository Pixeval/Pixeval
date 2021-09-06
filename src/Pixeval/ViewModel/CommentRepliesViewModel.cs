using System;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.CoreApi.Net.Response;

namespace Pixeval.ViewModel
{
    public class CommentRepliesViewModel : ObservableObject, ICommentViewModel
    {
        public CommentRepliesViewModel(IllustrationCommentsResponse.Comment comment)
        {
            Comment = comment;
        }

        public IllustrationCommentsResponse.Comment Comment { get; }

        public string? CommentContent => Comment.CommentContent;

        public DateTimeOffset PostDate => Comment.Date;

        private SoftwareBitmapSource? _avatarSource;

        public SoftwareBitmapSource? AvatarSource
        {
            get => _avatarSource;
            set => SetProperty(ref _avatarSource, value);
        }
    }
}