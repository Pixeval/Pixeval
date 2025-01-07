// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Windows.Foundation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.Controls.Windowing;
using WinUI3Utilities;

namespace Pixeval.Util.UI;

public static class ContentDialogBuilder
{
    public static IAsyncOperation<ContentDialogResult> CreateOkCancelAsync(this FrameworkElement owner, object? title, object? content, string? okButtonContent = null, string? cancelButtonContent = null)
    {
        return owner.ShowContentDialogAsync(
            title,
            content,
            okButtonContent ?? MessageContentDialogResources.OkButtonContent,
            cancelButtonContent ?? MessageContentDialogResources.CancelButtonContent);
    }

    public static IAsyncOperation<ContentDialogResult> CreateAcknowledgementAsync(this FrameworkElement owner, object? title, object? content, string? okButtonContent = null)
    {
        return owner.ShowContentDialogAsync(
            title,
            content,
            okButtonContent ?? MessageContentDialogResources.OkButtonContent);
    }

    public static IAsyncOperation<ContentDialogResult> CreateOkCancelAsync(this ulong hWnd, object? title, object? content, string? okButtonContent = null, string? cancelButtonContent = null)
    {
        return WindowFactory.ForkedWindows[hWnd].Content.To<FrameworkElement>().ShowContentDialogAsync(
            title,
            content,
            okButtonContent ?? MessageContentDialogResources.OkButtonContent,
            cancelButtonContent ?? MessageContentDialogResources.CancelButtonContent);
    }

    public static IAsyncOperation<ContentDialogResult> CreateAcknowledgementAsync(this ulong hWnd, object? title, object? content, string? okButtonContent = null)
    {
        return WindowFactory.ForkedWindows[hWnd].Content.To<FrameworkElement>().ShowContentDialogAsync(
            title,
            content,
            okButtonContent ?? MessageContentDialogResources.OkButtonContent);
    }

    public static IAsyncOperation<ContentDialogResult> ShowContentDialogAsync(this ulong hWnd, object? title, object? content, string primaryButtonText = "", string secondaryButtonText = "", string closeButtonText = "")
    {
        return WindowFactory.ForkedWindows[hWnd].Content.To<FrameworkElement>().ShowContentDialogAsync(
            title,
            content,
            primaryButtonText,
            secondaryButtonText,
            closeButtonText);
    }

    public static ContentDialog CreateContentDialog(this ulong hWnd, object? title, object? content, string primaryButtonText = "", string secondaryButtonText = "", string closeButtonText = "")
    {
        var cd = WindowFactory.ForkedWindows[hWnd].Content.To<FrameworkElement>().CreateContentDialog();
        cd.Title = title;
        cd.Content = content;
        cd.PrimaryButtonText = primaryButtonText;
        cd.SecondaryButtonText = secondaryButtonText;
        cd.CloseButtonText = closeButtonText;
        cd.DefaultButton = ContentDialogButton.Primary;
        return cd;
    }
}
