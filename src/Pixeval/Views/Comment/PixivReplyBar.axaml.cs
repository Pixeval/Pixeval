// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Pixeval.I18N;
using Pixeval.Utilities;

namespace Pixeval.Views;

public partial class PixivReplyBar : UserControl
{
    public PixivReplyBar() => InitializeComponent();

    public event Action<string>? SendButtonClick;

    public event Action<int>? StickerClick;

    private void EmojiImage_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Control { DataContext: int emojiId }) 
            return;
        var textBox = ReplyContentTextBox;
        var caretIndex = textBox.CaretIndex;
        var placeholder = ((PixivReplyEmoji)emojiId).PlaceholderKey;
        textBox.Text = (textBox.Text ?? "").Insert(caretIndex, placeholder);
        textBox.CaretIndex = caretIndex + placeholder.Length;
    }

    private void StickerImage_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Control { DataContext: int id })
            return;
        EmojiButton.Flyout?.Hide();
        StickerClick?.Invoke(id);
    }

    private void SendButton_OnClicked(object? sender, RoutedEventArgs e)
    {
        var content = ReplyContentTextBox.Text;
        if (string.IsNullOrEmpty(content) || content.Length > 140)
        {
            if (TopLevel.GetTopLevel(this)?.ViewContainer is not { } viewContainer)
                return;

            _ = viewContainer.CreateAcknowledgementAsync(
                I18NManager.GetResource(PixivReplyBarResources.CommentIsTooShortOrTooLongToastTitle),
                I18NManager.GetResource(PixivReplyBarResources.CommentIsTooShortOrTooLongToastContentFormatted,
                    content?.Length ?? 0));
            return;
        }

        SendButtonClick?.Invoke(content);
        ReplyContentTextBox.Clear();
    }
}
