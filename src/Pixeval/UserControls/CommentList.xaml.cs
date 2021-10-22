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
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Pixeval.Util.UI;
using Pixeval.ViewModel;

namespace Pixeval.UserControls
{
    public sealed partial class CommentList
    {
        public static DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(object),
            typeof(CommentList),
            PropertyMetadata.Create(Enumerable.Empty<CommentBlockViewModel>(), (o, args) => ((CommentList) o).CommentsList.ItemsSource = args.NewValue));

        private EventHandler<TappedRoutedEventArgs>? _repliesHyperlinkButtonTapped;

        public CommentList()
        {
            InitializeComponent();
        }

        public object ItemsSource
        {
            get => GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
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
}