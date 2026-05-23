// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

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

    private async void UserItem_OnTapped(object? sender, TappedEventArgs tappedEventArgs)
    {
        if (sender is not Control { DataContext: UserItemViewModel vm })
            return;
        if (TopLevel.GetTopLevel(this)?.ViewContainer is { } viewContainer)
            await viewContainer.CreateUserPageAsync(vm.UserId);
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

        if (DataContext is UserViewViewModel vm)
            RaiseEvent(new ViewModelDisposalEventArgs(ViewModelDisposal.ViewModelDisposalEvent, vm));
    }

    #endregion
}
