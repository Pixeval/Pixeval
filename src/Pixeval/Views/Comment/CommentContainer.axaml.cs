using Avalonia.Controls;
using Mako.Model;
using Pixeval.Utilities;
using Pixeval.ViewModels.Viewers;

namespace Pixeval.Views;

public partial class CommentContainer : ContentPage
{
    public CommentContainer() => InitializeComponent();

    public CommentContainer(CommentItemViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }

    private async void ReplyBar_OnSendButtonClick(string replyContent)
    {
        if (DataContext is not CommentsViewViewModel viewModel)
            return;
        AddComment(await viewModel.AddCommentAsync(replyContent), viewModel);
    }

    private async void ReplyBar_OnStickerClick(int stickerId)
    {
        if (DataContext is not CommentsViewViewModel viewModel)
            return;
        AddComment(await viewModel.AddStickerAsync(stickerId), viewModel);
    }

    private void AddComment(Comment comment, CommentsViewViewModel viewModel)
    {
        if (comment.Id is 0)
            TopLevel.GetTopLevel(this)?.ViewContainer?.ShowError("评论发送失败"); //TODO i18n
        else
            viewModel.AddComment(comment);
    }

    private async void CommentView_OnOpenRepliesButtonClick(CommentItemViewModel viewModel)
    {
        if (IsInNavigationPage && Parent is NavigationPage frame)
            await frame.PushAsync(new CommentContainer(viewModel));
    }
}
