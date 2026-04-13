// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Avalonia.Controls;
using Pixeval.ViewModels.Viewers;

namespace Pixeval.Views.Viewers;

public partial class UserViewerPage : ContentPage
{
    public UserViewerPage() : this(null)
    {
    }

    public UserViewerPage(UserViewerPageViewModel? viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
}
