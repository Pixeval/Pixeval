// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using AutoSettingsPage.Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CommunityToolkit.Avalonia.Controls;
using Pixeval.I18N;
using Pixeval.Models.Settings.Entries;
using Pixeval.Utilities;

namespace Pixeval.Views.Settings;

public partial class ProxySettingsExpander : SettingsExpander, IEntryControl<ProxyAppSettingsEntry>
{
    public ProxyAppSettingsEntry Entry { set => DataContext = value; }

    public ProxySettingsExpander() => InitializeComponent();

    private void TextBox_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (sender is not TextBox { Text: var text } || DataContext is not ProxyAppSettingsEntry entry)
            return;

        if (string.IsNullOrWhiteSpace(text))
        {
            entry.Proxy = null;
            return;
        }

        if (MakoHelper.NormalizeProxyUri(text) is not { } proxy)
        {
            TopLevel.GetTopLevel(this)?.ViewContainer?.ShowError(I18NManager.GetResource(SettingsMainViewResources.ProxyTextBoxErrorUri), text);
            entry.Proxy = null;
            return;
        }

        entry.Proxy = proxy;
    }
}
