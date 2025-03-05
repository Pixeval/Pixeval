// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Microsoft.UI.Xaml;
using Pixeval.Settings.Models;

namespace Pixeval.Controls.Settings;

public sealed partial class ClickableSettingsCard
{
    public ClickableAppSettingsEntry Entry { get; set; } = null!;

    public ClickableSettingsCard() => InitializeComponent();

    private void Clicked(object sender, RoutedEventArgs e) => Entry.Clicked();
}
