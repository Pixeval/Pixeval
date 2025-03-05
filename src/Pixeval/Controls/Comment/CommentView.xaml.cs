// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Pixeval.CoreApi.Global.Enum;

namespace Pixeval.Controls;

public sealed partial class CommentView : IStructuralDisposalCompleter
{
    [GeneratedDependencyProperty]
    public partial object? ItemsSource { get; set; }

    [GeneratedDependencyProperty(DefaultValue = true)]
    public partial bool HasNoItem { get; set; }

    public CommentView() => InitializeComponent();

    public event Action<CommentItemViewModel>? OpenRepliesButtonClick;

    public event Action<CommentItemViewModel>? DeleteButtonClick;

    public event Func<SimpleWorkType> RequireEntryType = null!;

    private void CommentItem_OnOpenRepliesButtonClick(CommentItemViewModel viewModel) => OpenRepliesButtonClick?.Invoke(viewModel);

    private void CommentItem_OnDeleteButtonClick(CommentItemViewModel viewModel) => DeleteButtonClick?.Invoke(viewModel);

    private SimpleWorkType CommentItem_OnRequireEntryType() => RequireEntryType();

    public void CompleteDisposal()
    {
        if (CommentsList.ItemsSource is IEnumerable<CommentItemViewModel> list)
            foreach (var commentBlockViewModel in list)
                commentBlockViewModel.Dispose();
    }

    public List<Action<IStructuralDisposalCompleter?>> ChildrenCompletes { get; } = [];

    public bool CompleterRegistered { get; set; }

    public bool CompleterDisposed { get; set; }

    private void CommentView_OnLoaded(object sender, RoutedEventArgs e) => ((IStructuralDisposalCompleter) this).Hook();
}
