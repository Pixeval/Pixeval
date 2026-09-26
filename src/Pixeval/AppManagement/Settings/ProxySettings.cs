// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using AutoSettingsPage;
using FluentIcons.Common;
using Pixeval.Models.Options;

namespace Pixeval.AppManagement.Settings;

public record ProxySettings
{
    public ProxyType ProxyType { get; set; }

    [SettingsEntry(Symbol.Server, AppSettingsResources.ProxyTextBoxEntry.Header, AppSettingsResources.ProxyTextBoxEntry.Description)]
    public string Proxy { get; set; } = "";
}
