using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media.Imaging;
using Pixeval.CoreApi.Net.Response;
using Pixeval.Util;
using Pixeval.Util.Generic;

namespace Pixeval.ViewModel
{
    public class CommentBlockViewModel : ObservableObject, IDisposable, ICommentViewModel
    {
        public CommentBlockViewModel(IllustrationCommentsResponse.Comment comment)
        {
            Comment = comment;
            Replies = new ObservableCollection<CommentRepliesViewModel>();
            if (HasReplies)
            {
                LoadReplies();
            }

            if (Comment.User?.ProfileImageUrls?.Medium is not null)
            {
                LoadAvatarSource();
            }

            if (Comment.Stamp?.StampUrl is not null)
            {
                LoadStampSource();
            }
        }

        public IllustrationCommentsResponse.Comment Comment { get; }

        public bool HasReplies => Comment.HasReplies;

        public bool IsStamp => Comment.Stamp is not null;

        public DateTimeOffset PostDate => Comment.Date;

        public string? CommentContent => Comment.CommentContent;

        private SoftwareBitmapSource? _avatarSource;

        public SoftwareBitmapSource? AvatarSource
        {
            get => _avatarSource;
            set => SetProperty(ref _avatarSource, value);
        }

        private SoftwareBitmapSource? _stampSource;

        public SoftwareBitmapSource? StampSource
        {
            get => _stampSource;
            set => SetProperty(ref _stampSource, value);
        }

        public ObservableCollection<CommentRepliesViewModel> Replies { get; }

        private async void LoadReplies()
        {
            Replies.AddRange(await App.MakoClient.IllustrationCommentReplies(Comment.Id.ToString())
                .Select(c => new CommentRepliesViewModel(c))
                .ToListAsync());

        }

        private async void LoadAvatarSource()
        {
            _avatarSource = await App.MakoClient.DownloadSoftwareBitmapSourceAsync(Comment.User!.ProfileImageUrls!.Medium!);
        }

        private async void LoadStampSource()
        {
            _stampSource = await App.MakoClient.DownloadSoftwareBitmapSourceAsync(Comment.Stamp!.StampUrl!);
        }

        public void Dispose()
        {
            DisposeInternal();
            GC.SuppressFinalize(this);
        }

        private void DisposeInternal()
        {
            StampSource?.Dispose();
            AvatarSource?.Dispose();
        }

        ~CommentBlockViewModel()
        {
            Dispose();
        }
    }
}