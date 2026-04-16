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

        if (DataContext is SpotlightViewViewModel vm)
            RaiseEvent(new ViewModelDisposalEventArgs(ViewModelDisposal.ViewModelDisposalEvent, vm));
    }

    #endregion
}
