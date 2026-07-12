// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Pixeval.Utilities;
using Pixeval.ViewModels;
using Pixeval.Views.Viewers;

namespace Pixeval.Views.Entry;

public partial class UserView : UserControl
{
    public UserView() => InitializeComponent();

    public void SetViewModel(UserViewViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        var oldViewModel = DataContext as IDisposable;
        DataContext = viewModel;
        oldViewModel?.Dispose();
        if (IsLoaded)
            RegisterViewModelDisposal(viewModel);
    }

    private void UserItem_OnTapped(object? sender, TappedEventArgs tappedEventArgs)
    {
        if (sender is not Control { DataContext: UserItemViewModel vm })
            return;
        if (TopLevel.GetTopLevel(this)?.ViewContainer is { } viewContainer)
            viewContainer.CreateUserPage(vm.UserId);
    }

    private void ListBox_OnContainerPrepared(object? sender, ContainerPreparedEventArgs e)
    {
        if (e.Container is ListBoxItem lbi)
            lbi.Tapped += UserItem_OnTapped;
    }

    #region Disposal

    /// <inheritdoc />
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (DataContext is UserViewViewModel viewModel)
            RegisterViewModelDisposal(viewModel);
    }

    private void RegisterViewModelDisposal(UserViewViewModel viewModel) =>
        RaiseEvent(new ViewModelDisposalEventArgs(ViewModelDisposal.ViewModelDisposalEvent, viewModel));

    #endregion
}
