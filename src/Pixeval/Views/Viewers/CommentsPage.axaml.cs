// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Avalonia.Controls;
using Pixeval.ViewModels.Viewers;

namespace Pixeval.Views.Viewers;

public partial class CommentsPage : NavigationPage
{
    private CommentsViewViewModel? ViewModel => DataContext as CommentsViewViewModel;

    public CommentsPage() : this(null) => InitializeComponent();

    public CommentsPage(CommentsViewViewModel? viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
        _ = PushAsync(new CommentContainer());
    }
}
