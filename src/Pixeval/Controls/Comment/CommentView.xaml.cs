#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/CommentView.xaml.cs
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

using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls;

[DependencyProperty<object>("ItemsSource")]
[DependencyProperty<bool>("HasNoItem", "true")]
[DependencyProperty<bool>("IsLoadingMore", "false")]
public sealed partial class CommentView
{
    public CommentView() => InitializeComponent();

    public event Action<CommentItemViewModel>? RepliesHyperlinkButtonClick;

    public event Action<CommentItemViewModel>? DeleteHyperlinkButtonClick;

    private void CommentItem_OnRepliesHyperlinkButtonClick(CommentItemViewModel viewModel)
    {
        RepliesHyperlinkButtonClick?.Invoke(viewModel);
    }

    private void CommentItem_OnDeleteHyperlinkButtonClick(CommentItemViewModel viewModel)
    {
        DeleteHyperlinkButtonClick?.Invoke(viewModel);
    }

    private void CommentView_OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (CommentsList.ItemsSource is IEnumerable<CommentItemViewModel> list)
            foreach (var commentBlockViewModel in list)
                commentBlockViewModel.Dispose();
    }
}
