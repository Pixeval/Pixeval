// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Reflection;
using AutoSettingsPage;
using AutoSettingsPage.Models;
using Avalonia.Controls;
using FluentIcons.Common;
using Pixeval.AppManagement;
using Pixeval.Controls;
using Pixeval.Models.Options;
using Pixeval.Utilities;

namespace Pixeval.Models.Settings.Entries;

public class ProxyAppSettingsEntry : EnumSettingsEntry<NetworkSettingsGroup, object>
{
    public ProxyAppSettingsEntry(NetworkSettingsGroup settings) : base(settings, t => t.ProxyType, SymbolComboBoxItem.GetValues<ProxyType>())
    {
        ValueChanged += _ =>
        {
            OnPropertyChanged(nameof(IsProxyTextBoxEnabled));
            OnProxyChanged();
        };

        Token2 = nameof(NetworkSettingsGroup.Proxy);
        var member = typeof(NetworkSettingsGroup).GetProperty(Token2);

        if (member?.GetCustomAttribute<SettingsEntryAttribute>() is { } attribute)
        {
            Header2 = attribute.Header;
            Description2 = attribute.Description;
            Icon2 = attribute.Icon;
        }
    }

    #region Entry2

    public Symbol Icon2 { get; set; }

    public string Header2 { get; set; } = "";

    public object DescriptionControl2
    {
        get
        {
            if (DescriptionUri2 is not null)
            {
                var b = new HyperlinkButton
                {
                    Padding = new(0),
                    Content = Description2,
                    NavigateUri = DescriptionUri2
                };
                return b;
            }

            return Description2;
        }
    }

    public string Description2 { get; set; } = "";

    public Uri? DescriptionUri2 { get; set; }

    public string Token2 { get; }

    #endregion

    public event Action<string?>? ProxyChanged;

    public bool IsProxyTextBoxEnabled => (ProxyType) Value is ProxyType.Custom;

    public string? Proxy
    {
        get => Settings.Proxy;
        set
        {
            value = MakoHelper.NormalizeProxyUri(value) ?? "";
            if (Settings.Proxy == value)
                return;
            Settings.Proxy = value;
            OnPropertyChanged();
            OnProxyChanged();
        }
    }

    private void OnProxyChanged() => ProxyChanged?.Invoke(MakoHelper.ToMakoProxy((ProxyType) Value, Proxy));
}
