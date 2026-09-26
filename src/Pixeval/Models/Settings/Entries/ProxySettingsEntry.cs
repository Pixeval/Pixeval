// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using AutoSettingsPage.Models;
using Pixeval.AppManagement.Settings;
using Pixeval.Controls;
using Pixeval.Models.Options;
using Pixeval.Utilities;

namespace Pixeval.Models.Settings.Entries;

public class ProxySettingsEntry : MultiValuesWithMainValueEntry<NetworkSettingsGroup, ProxySettings, EnumSettingsEntry<ProxySettings, object>>
{
    public ProxySettingsEntry(NetworkSettingsGroup settings)
        : this(
            settings.ProxySettings,
            new EnumSettingsEntry<ProxySettings, object>(
                settings.ProxySettings,
                t => (object) t.ProxyType,
                SymbolComboBoxItem.GetValues<ProxyType>()),
            new StringSettingsEntry<ProxySettings>(settings.ProxySettings, t => t.Proxy))
    {
    }

    private ProxySettingsEntry(
        ProxySettings settings,
        EnumSettingsEntry<ProxySettings, object> mainValue,
        StringSettingsEntry<ProxySettings> proxyEntry)
        : base(settings, t => t.ProxySettings, mainValue, [proxyEntry])
    {
        MainValue.ValueChanged += _ => OnProxyChanged();
        proxyEntry.ValueChanged += _ => OnProxyChanged();
    }

    public event Action<string?>? ProxyChanged;

    private void OnProxyChanged() => ProxyChanged?.Invoke(MakoHelper.ToMakoProxy((ProxyType) MainValue.Value, Settings.Proxy));
}
