using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace Pixeval.ViewModel
{
    public class CommentRepliesBlockViewModel : ObservableObject
    {
        public CommentRepliesBlockViewModel(CommentBlockViewModel comment)
        {
            Comment = comment;
        }

        public CommentBlockViewModel Comment { get; }

        public bool HasReplies => Comment.HasReplies;
    }
}