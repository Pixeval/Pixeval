// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Collections.ObjectModel;
using AutoSettingsPage.Avalonia;
using AutoSettingsPage.Models;
using CommunityToolkit.Avalonia.Controls;

namespace Pixeval.Views.Settings;

public partial class StringCollectionSettingsExpander : SettingsExpander, IEntryControl<ISingleValueSettingsEntry<ObservableCollection<string>>>
{
    public ISingleValueSettingsEntry<ObservableCollection<string>> Entry { set => DataContext = value; }

    public StringCollectionSettingsExpander() => InitializeComponent();
}
