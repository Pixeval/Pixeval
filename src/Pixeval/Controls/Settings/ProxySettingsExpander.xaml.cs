using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.Controls.Windowing;
using Pixeval.Settings.Models;
using WinUI3Utilities;

namespace Pixeval.Controls.Settings;

public sealed partial class ProxySettingsExpander
{
    public ProxyAppSettingsEntry Entry { get; set; } = null!;

    public ProxySettingsExpander() => InitializeComponent();

    private void TextBox_OnLostFocus(object sender, RoutedEventArgs e)
    {
        var text = sender.To<TextBox>().Text;

        if (string.IsNullOrWhiteSpace(text))
        {
            Entry.Proxy = null;
            return;
        }

        var proxy = text;

        if (!text.Contains("://"))
            proxy = "http://" + proxy;

        if (!Uri.IsWellFormedUriString(proxy, UriKind.Absolute))
        {
            WindowFactory.GetWindowForElement(this).HWnd.ErrorGrowl(SettingsPageResources.ProxyTextBoxErrorUri, proxy);
            Entry.Proxy = null;
            return;
        }

        Entry.Proxy = proxy;

        Entry.ProxyChanged?.Invoke(Entry.MakoProxy);
    }

    private void EnumComboBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        Entry.ValueChanged?.Invoke(Entry.Value);
    }
}
