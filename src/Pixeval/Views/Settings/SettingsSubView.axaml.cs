// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using AutoSettingsPage.Models;
using Avalonia.Controls;

namespace Pixeval.Views.Settings;

public partial class SettingsSubView : ContentPage
{
    public SettingsSubView() => InitializeComponent();

    public SettingsSubView(ISettingsGroup group) : this()
    {
        Header = group.Header;
        ItemsControl.ItemsSource = group;
    }
}
