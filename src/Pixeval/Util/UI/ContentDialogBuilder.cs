// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;
using WinUI3Utilities;

namespace Pixeval.Util.UI;

public static class ContentDialogBuilder
{
    extension(FrameworkElement frameworkElement)
    {
        public IAsyncOperation<ContentDialogResult> CreateOkCancelAsync(object? title, object? content, string? okButtonContent = null, string? cancelButtonContent = null)
        {
            return frameworkElement.ShowContentDialogAsync(
                title,
                content,
                okButtonContent ?? MessageContentDialogResources.OkButtonContent,
                cancelButtonContent ?? MessageContentDialogResources.CancelButtonContent);
        }

        public IAsyncOperation<ContentDialogResult> CreateAcknowledgementAsync(object? title, object? content, string? okButtonContent = null)
        {
            return frameworkElement.ShowContentDialogAsync(
                title,
                content,
                okButtonContent ?? MessageContentDialogResources.OkButtonContent);
        }

        public IAsyncOperation<ContentDialogResult> ShowDialogAsync(object? title, object? content, string primaryButtonText = "", string secondaryButtonText = "", string closeButtonText = "")
        {
            return frameworkElement.ShowContentDialogAsync(
                title,
                content,
                primaryButtonText,
                secondaryButtonText,
                closeButtonText);
        }

        public ContentDialog CreateDialog(object? title, object? content, string primaryButtonText = "", string secondaryButtonText = "", string closeButtonText = "")
        {
            var cd = frameworkElement.CreateContentDialog();
            cd.Title = title;
            cd.Content = content;
            cd.PrimaryButtonText = primaryButtonText;
            cd.SecondaryButtonText = secondaryButtonText;
            cd.CloseButtonText = closeButtonText;
            cd.DefaultButton = ContentDialogButton.Primary;
            return cd;
        }
    }
}
