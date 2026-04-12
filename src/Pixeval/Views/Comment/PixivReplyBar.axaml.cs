// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Pixeval.Views.Comment;

public partial class PixivReplyBar : UserControl
{
    public PixivReplyBar() => InitializeComponent();

    public event EventHandler<SendButtonClickEventArgs>? SendButtonClick;

    private void SendButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        var content = ReplyContentTextBox.Text;
        if (string.IsNullOrEmpty(content) || content.Length > 140)
            return;

        SendButtonClick?.Invoke(this, new SendButtonClickEventArgs(e, content));
        ReplyContentTextBox.Clear();
    }
}

public class SendButtonClickEventArgs(RoutedEventArgs clickEventArgs, string replyContent) : EventArgs
{
    public RoutedEventArgs ClickEventArgs { get; } = clickEventArgs;

    public string ReplyContent { get; } = replyContent;
}
