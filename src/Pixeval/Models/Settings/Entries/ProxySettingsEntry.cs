// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using AutoSettingsPage.Models;
using Pixeval.AppManagement;
using Pixeval.Controls;
using Pixeval.Models.Options;
using Pixeval.Utilities;

namespace Pixeval.Models.Settings.Entries;

public class ProxySettingsEntry : MultiValuesWithMainValueEntry<NetworkSettingsGroup, EnumSettingsEntry<NetworkSettingsGroup, object>>
{
    public ProxySettingsEntry(NetworkSettingsGroup settings)
        : this(
            settings,
            new EnumSettingsEntry<NetworkSettingsGroup, object>(
                settings,
                t => (object) t.ProxyType,
                SymbolComboBoxItem.GetValues<ProxyType>()),
            new StringSettingsEntry<NetworkSettingsGroup>(settings, t => t.Proxy))
    {
    }

    private ProxySettingsEntry(
        NetworkSettingsGroup settings,
        EnumSettingsEntry<NetworkSettingsGroup, object> mainValue,
        StringSettingsEntry<NetworkSettingsGroup> proxyEntry)
        : base(settings, mainValue, [proxyEntry])
    {
        MainValue.ValueChanged += _ => OnProxyChanged();
        proxyEntry.ValueChanged += _ => OnProxyChanged();
    }

    public event Action<string?>? ProxyChanged;

    private void OnProxyChanged() => ProxyChanged?.Invoke(MakoHelper.ToMakoProxy((ProxyType) MainValue.Value, Settings.Proxy));
}
