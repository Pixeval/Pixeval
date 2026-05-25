// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

namespace Pixeval.Views.Home;

public sealed partial class HomePageCardControl
{
    private async void HomePageCardControl_OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Loaded -= HomePageCardControl_OnLoaded;
        if (PreviewViewModel is not null)
            await PreviewViewModel.LoadAsync();
    }
}
