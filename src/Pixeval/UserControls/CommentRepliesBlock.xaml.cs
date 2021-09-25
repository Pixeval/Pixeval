using System;
using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Pixeval.Util.UI;
using Pixeval.ViewModel;

namespace Pixeval.UserControls
{
    public sealed partial class CommentRepliesBlock
    {
        public static DependencyProperty ViewModelProperty = DependencyProperty.Register(
            nameof(ViewModel),
            typeof(CommentRepliesBlockViewModel),
            typeof(CommentRepliesBlock),
            PropertyMetadata.Create(DependencyProperty.UnsetValue, ViewModelChangedCallback));

        private static void ViewModelChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var block = (CommentRepliesBlock) d;
            var viewModel = (CommentRepliesBlockViewModel) e.NewValue;
            block.RepliesAreEmptyPanel.Visibility = (!viewModel.HasReplies).ToVisibility();
            block.CommentList.Visibility = viewModel.HasReplies.ToVisibility();
            if (viewModel.HasReplies && viewModel.Comment.Replies is { } rs)
            {
                block.CommentList.ItemsSource = rs;
            }
        }

        public CommentRepliesBlockViewModel ViewModel
        {
            get => (CommentRepliesBlockViewModel) GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public CommentRepliesBlock()
        {
            InitializeComponent();
        }

        private EventHandler<TappedRoutedEventArgs>? _closeButtonTapped;

        public event EventHandler<TappedRoutedEventArgs> CloseButtonTapped
        {
            add => _closeButtonTapped += value;
            remove => _closeButtonTapped -= value;
        }

        private void CloseButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            _closeButtonTapped?.Invoke(sender, e);
        }

        private void CommentList_OnRepliesHyperlinkButtonTapped(object? sender, TappedRoutedEventArgs e)
        {
            ReplyBar.FindDescendant<RichEditBox>()?.Focus(FocusState.Programmatic);
        }
    }
}
