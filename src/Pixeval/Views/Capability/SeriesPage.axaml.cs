// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Avalonia.Interactivity;
using Mako.Global.Enum;
using Pixeval.Controls;
using Pixeval.ViewModels;

namespace Pixeval.Views.Capability;

public partial class SeriesPage : IconContentPage
{

    public SeriesPage()
    {
        InitializeComponent();
        ChangeSource();
    }

    private void SimpleWorkTypeComboBox_OnSelectionChanged(SymbolComboBox sender, EventArgs e)
    {
        ChangeSource();
    }

    private void RefreshButton_OnClick(object? sender, RoutedEventArgs e) => ChangeSource();

    private void ChangeSource()
    {
        var workType = SimpleWorkTypeComboBox.GetSelectedValue<SimpleWorkType>();
        (SeriesView.DataContext as SeriesViewViewModel)?.ResetEngine(
            App.AppViewModel.MakoClient.WorkSeriesWatchlist(workType),
            workType);
    }
}
