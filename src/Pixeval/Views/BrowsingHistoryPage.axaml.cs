// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Avalonia.Controls;
using Mako.Global.Enum;
using Misaki;
using Pixeval.Controls;

namespace Pixeval.Views;

public partial class BrowsingHistoryPage : ContentPage
{
    public BrowsingHistoryPage()
    {
        InitializeComponent();
        WorkContainer.SetSource(App.AppViewModel.HistoryPersistHelper.BrowseHistoryEntries);
        // SimpleWorkTypeComboBox_OnSelectionChanged(SimpleWorkTypeComboBox, EventArgs.Empty);
    }

    //private void SimpleWorkTypeComboBox_OnSelectionChanged(SymbolComboBox sender, EventArgs e)
    //{
    //    var type = sender.GetSelectedValue<SimpleWorkType>();
    //    WorkContainer.Filter = entry => entry.Entry.ImageType is ImageType.Other ^ type is SimpleWorkType.IllustrationAndManga;
    //}
}
