using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Pixeval.Utilities;
using Pixeval.ViewModels;
using Pixeval.Views.Viewers;

namespace Pixeval.Views.Entry;

public partial class UserView : UserControl, IStructuralDisposalCompleter
{
    public UserView() => InitializeComponent();

    private async void UserItem_OnDataContextChanged(object? sender, EventArgs e)
    {
        if (sender is not Control { DataContext: UserItemViewModel viewModel })
            return;
        await viewModel.LoadAvatarAsync();
    }

    private async void UserItem_OnTapped(object? sender, TappedEventArgs tappedEventArgs)
    {
        if (sender is not Control { DataContext: UserItemViewModel vm })
            return;
        if (TopLevel.GetTopLevel(this)?.ViewContainer is { } viewContainer)
            await viewContainer.CreateUserPageAsync(vm.UserId);
    }

    public void CompleteDisposal()
    {
        var d = DataContext;
        DataContext = null!;
        if (d is not UserViewViewModel viewModel)
            return;
        viewModel.Dispose();
    }

    public List<Action<IStructuralDisposalCompleter?>> ChildrenCompletes { get; } = [];

    public bool CompleterRegistered { get; set; }

    public bool CompleterDisposed { get; set; }

    /// <inheritdoc />
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        ((IStructuralDisposalCompleter) this).Hook();
    }

    private void ListBox_OnContainerPrepared(object? sender, ContainerPreparedEventArgs e)
    {
        if (e.Container is ListBoxItem lbi)
            lbi.Tapped += UserItem_OnTapped;
    }
}
