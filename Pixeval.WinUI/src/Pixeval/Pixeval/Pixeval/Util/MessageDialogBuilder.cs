using Microsoft.UI.Xaml.Controls;

namespace Pixeval.Util
{
    public class MessageDialogBuilder
    {
        private readonly ContentDialog _contentDialog = new();

        public static MessageDialogBuilder Create() => new();

        public MessageDialogBuilder WithTitle(string title)
        {
            _contentDialog.Title = title;
            return this;
        }

        public MessageDialogBuilder WithPrimaryButtonText(string text)
        {
            _contentDialog.PrimaryButtonText = text;
            return this;
        }

        public MessageDialogBuilder WithSecondaryButtonText(string text)
        {
            _contentDialog.SecondaryButtonText = text;
            return this;
        }

        public MessageDialogBuilder WithCloseButtonText(string text)
        {
            _contentDialog.CloseButtonText = text;
            return this;
        }

        public MessageDialogBuilder WithDefaultButton(ContentDialogButton button)
        {
            _contentDialog.DefaultButton = button;
            return this;
        }

        public MessageDialogBuilder WithContent(string message)
        {
            _contentDialog.Content = new TextBlock
            {
                Text = message
            };
            return this;
        }

        public ContentDialog Build()
        {
            return _contentDialog;
        }
    }
}