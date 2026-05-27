// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Avalonia.Controls;
using Pixeval.Controls;
using Pixeval.ViewModels;

namespace Pixeval.Views.Capability;

public partial class SpotlightsPage : ContentPage
{
    public SpotlightsPage() : this(null)
    {
    }

    public SpotlightsPage(SpotlightViewViewModel? viewModel)
    {
        InitializeComponent();
        if (viewModel is not null)
        {
            var oldViewModel = SpotlightView.DataContext as IDisposable;
            SpotlightView.DataContext = viewModel;
            oldViewModel?.Dispose();
        }
        else
        {
            ChangeSource();
        }
    }

    private void SpotlightCategoryComboBox_OnSelectionChanged(SymbolComboBox sender, EventArgs e)
    {
        ChangeSource();
    }

    private void ChangeSource()
    {
        (SpotlightView.DataContext as SpotlightViewViewModel)?.ResetEngine(App.AppViewModel.MakoClient.Spotlight(), (spotlight, _) => new(spotlight));
    }
}
