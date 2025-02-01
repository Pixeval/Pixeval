// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;

namespace Pixeval.Controls;

public sealed partial class CommentView : IStructuralDisposalCompleter
{
    [GeneratedDependencyProperty]
    public partial object? ItemsSource { get; set; }

    [GeneratedDependencyProperty(DefaultValue = true)]
    public partial bool HasNoItem { get; set; }

    [GeneratedDependencyProperty(DefaultValue = false)]
    public partial bool IsLoadingMore { get; set; }

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

    public void CompleteDisposal()
    {
       if (CommentsList.ItemsSource is IEnumerable<CommentItemViewModel> list)
            foreach (var commentBlockViewModel in list)
                commentBlockViewModel.Dispose();
    }

    public List<Action> ChildrenCompletes { get; } = [];

    public bool CompleterRegistered { get; set; }

    public bool CompleterDisposed { get; set; }

    private void CommentView_OnLoaded(object sender, RoutedEventArgs e)
    {
        ((IStructuralDisposalCompleter) this).Hook();
    }
}
