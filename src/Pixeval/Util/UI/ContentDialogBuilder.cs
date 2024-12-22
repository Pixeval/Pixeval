#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/MessageDialogBuilder.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

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
