using System;
using Avalonia.Controls;
using Avalonia.Input;
using Pixeval.ViewModels;

namespace Pixeval.Views.Entry;

public partial class SpotlightView : UserControl
{
    public SpotlightView()
    {
        InitializeComponent();
    }

    private async void SpotlightItem_OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is not { } viewModel
            || sender is not Control { DataContext: SpotlightItemViewModel vm })
            return;
        _ = await vm.TryLoadThumbnailAsync(viewModel);
    }

    private async void SpotlightItem_OnTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not Control { DataContext: SpotlightItemViewModel vm } control)
            return;
        TopLevel.GetTopLevel(control)?.Launcher.LaunchUriAsync(new Uri(vm.Entry.ArticleUrl));
    }

    private void ListBox_OnContainerPrepared(object? sender, ContainerPreparedEventArgs e)
    {
        if (e.Container is ListBoxItem lbi)
            lbi.Tapped += SpotlightItem_OnTapped;
    }
}
