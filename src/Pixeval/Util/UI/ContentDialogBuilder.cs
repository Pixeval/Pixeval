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

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Pixeval.Util.UI;

public static class ContentDialogBuilder
{
    public static ContentDialog CreateOkCancel(this UIElement owner, object? title, object? content)
    {
        return new ContentDialog()
            .WithTitle(title)
            .WithContent(content)
            .WithPrimaryButtonText(MessageContentDialogResources.OkButtonContent)
            .WithCloseButtonText(MessageContentDialogResources.CancelButtonContent)
            .WithDefaultButton(ContentDialogButton.Primary)
            .Build(owner);
    }

    public static ContentDialog CreateAcknowledgement(this UIElement owner, object? title, object? content)
    {
        return new ContentDialog()
            .WithTitle(title)
            .WithContent(content)
            .WithPrimaryButtonText(MessageContentDialogResources.OkButtonContent)
            .WithDefaultButton(ContentDialogButton.Primary)
            .Build(owner);
    }

    public static ContentDialog WithTitle(this ContentDialog contentDialog, object? title)
    {
        contentDialog.Title = title;
        return contentDialog;
    }

    public static ContentDialog WithPrimaryButtonText(this ContentDialog contentDialog, string? text)
    {
        contentDialog.PrimaryButtonText = text;
        return contentDialog;
    }

    public static ContentDialog WithSecondaryButtonText(this ContentDialog contentDialog, string? text)
    {
        contentDialog.SecondaryButtonText = text;
        return contentDialog;
    }

    public static ContentDialog WithCloseButtonText(this ContentDialog contentDialog, string? text)
    {
        contentDialog.CloseButtonText = text;
        return contentDialog;
    }

    public static ContentDialog WithDefaultButton(this ContentDialog contentDialog, ContentDialogButton button)
    {
        contentDialog.DefaultButton = button;
        return contentDialog;
    }

    public static ContentDialog WithContent(this ContentDialog contentDialog, object? content)
    {
        contentDialog.Content = content;
        return contentDialog;
    }

    public static ContentDialog Build(this ContentDialog contentDialog, UIElement owner) // the owner argument is a workaround for issue #4870
    {
        contentDialog.XamlRoot = owner.XamlRoot;
        return contentDialog;
    }
}
