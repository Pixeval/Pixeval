// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Pixeval.Utilities;
using Pixeval.ViewModels.Viewers;

namespace Pixeval.Views.Viewers;

public partial class UserViewerPage : ContentPage
{
    private UserViewerPageViewModel ViewModel => (UserViewerPageViewModel) DataContext!;

    public UserViewerPage() : this(null)
    {
    }

    public UserViewerPage(UserViewerPageViewModel? viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }

    #region Disposal

    /// <inheritdoc />
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        RaiseEvent(new ViewModelDisposalEventArgs(ViewModelDisposal.ViewModelDisposalEvent, ViewModel));
    }

    #endregion

    private void UserInfoExpander_OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if(e.Property != Expander.IsExpandedProperty)
            return;

    }
}
