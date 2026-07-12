// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Pixeval.Utilities;
using Pixeval.ViewModels;

namespace Pixeval.Views.Entry;

public partial class SpotlightView : UserControl
{
    public SpotlightView() => InitializeComponent();

    public void SetViewModel(SpotlightViewViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        var oldViewModel = DataContext as IDisposable;
        DataContext = viewModel;
        oldViewModel?.Dispose();
        if (IsLoaded)
            RegisterViewModelDisposal(viewModel);
    }

    private async void SpotlightItem_OnTapped(object? sender, TappedEventArgs e)
    {
        if (sender is Control { DataContext: SpotlightItemViewModel vm } control)
            TopLevel.GetTopLevel(control)?.Launcher.LaunchUriAsync(new Uri(vm.Entry.ArticleUrl));
    }

    private void ListBox_OnContainerPrepared(object? sender, ContainerPreparedEventArgs e)
    {
        if (e.Container is ListBoxItem lbi)
            lbi.Tapped += SpotlightItem_OnTapped;
    }

    #region Disposal

    /// <inheritdoc />
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (DataContext is SpotlightViewViewModel viewModel)
            RegisterViewModelDisposal(viewModel);
    }

    private void RegisterViewModelDisposal(SpotlightViewViewModel viewModel) =>
        RaiseEvent(new ViewModelDisposalEventArgs(ViewModelDisposal.ViewModelDisposalEvent, viewModel));

    #endregion
}
