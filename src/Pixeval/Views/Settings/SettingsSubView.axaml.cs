// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using AutoSettingsPage.Models;
using Avalonia.Controls;
using FluentIcons.Avalonia;
using FluentIcons.Common;

namespace Pixeval.Views.Settings;

public partial class SettingsSubView : ContentPage
{
    public SettingsSubView() => InitializeComponent();

    public SettingsSubView(ISettingsGroup group) : this()
    {
        Icon = new SymbolIcon
        {
            Symbol = group.Icon,
            FontSize = 16,
            IconVariant = IconVariant.Color
        };
        Header = group.Header;
        ItemsControl.ItemsSource = group;
    }
}
