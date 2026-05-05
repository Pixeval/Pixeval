// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Mako.Global.Enum;
using Mako.Model;
using Misaki;
using Pixeval.Controls;

namespace Pixeval.Views;

public partial class BrowsingHistoryPage : ContentPage
{
    public BrowsingHistoryPage()
    {
        InitializeComponent();
        ChangeSource();
    }

    private void SimpleWorkTypeComboBox_OnSelectionChanged(SymbolComboBox sender, EventArgs e) => ChangeSource();

    private void WorkContainer_OnRefreshRequested(object? sender, RoutedEventArgs e) => ChangeSource();

    private void ChangeSource()
    {
        var type = SimpleWorkTypeComboBox.GetSelectedValue<SimpleWorkType>();
        var isIllustration = type is SimpleWorkType.IllustrationAndManga;
        var source = App.AppViewModel.HistoryPersistHelper.BrowseHistoryEntries
            .Reverse()
            .Where(t => t.ImageType is ImageType.Other ^ isIllustration)
            .ToAsyncEnumerable();

        WorkContainer.ResetEngine(type switch
        {
            SimpleWorkType.IllustrationAndManga => App.AppViewModel.MakoClient.Computed(source.OfType<Illustration>()),
            _ => App.AppViewModel.MakoClient.Computed(source.OfType<Novel>())
        }, false);
    }
}
