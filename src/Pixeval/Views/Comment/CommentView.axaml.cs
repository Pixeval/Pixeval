// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Avalonia.Controls;
using Mako.Global.Enum;
using Pixeval.ViewModels.Viewers;

namespace Pixeval.Views.Comment;

public partial class CommentView : UserControl
{
    public CommentView() => InitializeComponent();

    public event Action<CommentItemViewModel>? OpenRepliesButtonClick;

    public event Action<CommentItemViewModel>? DeleteButtonClick;

    public event Func<SimpleWorkType>? RequireEntryType;

    private void CommentItem_OnOpenRepliesButtonClick(CommentItemViewModel viewModel) => OpenRepliesButtonClick?.Invoke(viewModel);

    private void CommentItem_OnDeleteButtonClick(CommentItemViewModel viewModel) => DeleteButtonClick?.Invoke(viewModel);

    private SimpleWorkType CommentItem_OnRequireEntryType() => RequireEntryType?.Invoke() ?? SimpleWorkType.IllustrationAndManga;
}
