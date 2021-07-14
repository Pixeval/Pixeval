using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.Pages;

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
            _contentDialog.Content = new MessageDialogContent(message);
            return this;
        }

        public ContentDialog Build(Page owner) // the owner argument is a workaround for issue #4870
        {
            _contentDialog.XamlRoot = owner.Content.XamlRoot;
            return _contentDialog;
        }

        public ContentDialog Build(Window owner) // the owner argument is a workaround for issue #4870
        {
            _contentDialog.XamlRoot = owner.Content.XamlRoot;
            return _contentDialog;
        }
    }
}