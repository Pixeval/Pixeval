using System;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
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

        public event EventHandler<TappedRoutedEventArgs> RepliesHyperlinkButtonTapped
        {
            add => _repliesHyperlinkButtonTapped += value;
            remove => _repliesHyperlinkButtonTapped -= value;
        }

        public object ItemsSource
        {
            get => GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public CommentList()
        {
            InitializeComponent();
        }

        private void CommentBlock_OnRepliesHyperlinkButtonTapped(object? sender, TappedRoutedEventArgs e)
        {
            _repliesHyperlinkButtonTapped?.Invoke(sender, e);
        }
    }
}
