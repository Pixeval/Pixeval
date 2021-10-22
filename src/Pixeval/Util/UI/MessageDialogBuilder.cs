#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/MessageDialogBuilder.cs
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
using Pixeval.Pages.Misc;

namespace Pixeval.Util.UI
{
    public class MessageDialogBuilder
    {
        private readonly ContentDialog _contentDialog = new();

        public static MessageDialogBuilder Create()
        {
            return new MessageDialogBuilder();
        }

        public static ContentDialog CreateOkCancel(UserControl owner, string title, string content)
        {
            return Create().WithTitle(title)
                .WithContent(content)
                .WithPrimaryButtonText(MessageContentDialogResources.OkButtonContent)
                .WithCloseButtonText(MessageContentDialogResources.CancelButtonContent)
                .WithDefaultButton(ContentDialogButton.Primary)
                .Build(owner);
        }

        public static ContentDialog CreateOkCancel(Window owner, string title, string content)
        {
            return Create().WithTitle(title)
                .WithContent(content)
                .WithPrimaryButtonText(MessageContentDialogResources.OkButtonContent)
                .WithCloseButtonText(MessageContentDialogResources.CancelButtonContent)
                .WithDefaultButton(ContentDialogButton.Primary)
                .Build(owner);
        }

        public static ContentDialog CreateAcknowledgement(UserControl owner, string title, string content)
        {
            return Create().WithTitle(title)
                .WithContent(content)
                .WithPrimaryButtonText(MessageContentDialogResources.OkButtonContent)
                .WithDefaultButton(ContentDialogButton.Primary)
                .Build(owner);
        }

        public static ContentDialog CreateAcknowledgement(Window owner, string title, string content)
        {
            return Create().WithTitle(title)
                .WithContent(content)
                .WithPrimaryButtonText(MessageContentDialogResources.OkButtonContent)
                .WithDefaultButton(ContentDialogButton.Primary)
                .Build(owner);
        }

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

        public ContentDialog Build(UserControl owner) // Remarks: the owner argument is a workaround for issue #4870
        {
            _contentDialog.XamlRoot = owner.Content.XamlRoot;
            return _contentDialog;
        }

        public ContentDialog Build(Window owner) // Remarks: the owner argument is a workaround for issue #4870
        {
            _contentDialog.XamlRoot = owner.Content.XamlRoot;
            return _contentDialog;
        }
    }
}