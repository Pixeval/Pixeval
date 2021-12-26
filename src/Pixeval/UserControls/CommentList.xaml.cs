#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/CommentList.xaml.cs
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
using Microsoft.UI.Xaml.Input;
using Pixeval.Attributes;
using Pixeval.Misc;
using Pixeval.Pages.IllustrationViewer;
using Pixeval.Util.UI;

namespace Pixeval.UserControls;

[DependencyProperty("ItemsSource", typeof(object), nameof(OnItemsSourceChanged), DefaultValue = "System.Linq.Enumerable.Empty<Pixeval.Pages.IllustrationViewer.CommentBlockViewModel>()")]
public sealed partial class CommentList
{
    private EventHandler<TappedRoutedEventArgs>? _repliesHyperlinkButtonTapped;

    public CommentList()
    {
        InitializeComponent();
    }

    private static void OnItemsSourceChanged(DependencyObject o, DependencyPropertyChangedEventArgs args)
    {
        ((CommentList) o).CommentsList.ItemsSource = args.NewValue;
    }


    public event EventHandler<TappedRoutedEventArgs> RepliesHyperlinkButtonTapped
    {
        add => _repliesHyperlinkButtonTapped += value;
        remove => _repliesHyperlinkButtonTapped -= value;
    }

    private void CommentBlock_OnRepliesHyperlinkButtonTapped(object? sender, TappedRoutedEventArgs e)
    {
        _repliesHyperlinkButtonTapped?.Invoke(sender, e);
    }

    private void CommentBlock_OnDeleteHyperlinkButtonTapped(object? sender, TappedRoutedEventArgs e)
    {
        var viewModel = sender!.GetDataContext<CommentBlockViewModel>();
        App.AppViewModel.MakoClient.DeleteCommentAsync(viewModel.CommentId);
        if (ItemsSource is IList<CommentBlockViewModel> list)
        {
            list.Remove(viewModel);
        }
    }
}