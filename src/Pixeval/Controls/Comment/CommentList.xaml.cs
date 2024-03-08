#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/CommentList.xaml.cs
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
public sealed partial class CommentList
{
    public CommentList() => InitializeComponent();

    public event Action<CommentBlockViewModel>? RepliesHyperlinkButtonTapped;

    private void CommentBlock_OnRepliesHyperlinkButtonTapped(CommentBlockViewModel viewModel)
    {
        RepliesHyperlinkButtonTapped?.Invoke(viewModel);
    }

    private void CommentBlock_OnDeleteHyperlinkButtonTapped(CommentBlockViewModel viewModel)
    {
        _ = App.AppViewModel.MakoClient.DeleteIllustCommentAsync(viewModel.CommentId);
        if (CommentsList.ItemsSource is IList<CommentBlockViewModel> list)
            _ = list.Remove(viewModel);
    }

    private void CommentList_OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (CommentsList.ItemsSource is IEnumerable<CommentBlockViewModel> list)
            foreach (var commentBlockViewModel in list)
                commentBlockViewModel.Dispose();
    }
}
