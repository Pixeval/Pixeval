using System;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Pixeval.ViewModel
{
    public interface ICommentViewModel
    { 
        string? CommentContent { get; }

        SoftwareBitmapSource? AvatarSource { get; }

        DateTimeOffset PostDate { get; }
    }
}