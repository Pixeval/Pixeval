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

namespace Pixeval.Models.Settings.Entries;

public class ProxyAppSettingsEntry : EnumSettingsEntry<AppSettings, object>
{
    public ProxyAppSettingsEntry(AppSettings settings) : base(settings, t => t.ProxyType, SymbolComboBoxItem.GetValues<ProxyType>())
    {
        Token2 = nameof(AppSettings.Proxy);
        var member = typeof(AppSettings).GetProperty(Token2);

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

    public Action<string?>? ProxyChanged { get; set; }

    public string? Proxy
    {
        get => Settings.Proxy;
        set
        {
            if (Settings.Proxy == value)
                return;
            Settings.Proxy = value ?? "";
            OnPropertyChanged();
            ProxyChanged?.Invoke(MakoProxy);
        }
    }

    private string? MakoProxy
    {
        get
        {
            string scheme;
            switch (Value)
            {
                case ProxyType.System:
                    return "";
                case ProxyType.Http:
                    scheme = "http";
                    break;
                case ProxyType.Socks4:
                    scheme = "socks4";
                    break;
                case ProxyType.Socks4A:
                    scheme = "socks4a";
                    break;
                case ProxyType.Socks5:
                    scheme = "socks5";
                    break;
                default:
                    return null;
            }

            if (!Uri.TryCreate(Proxy, UriKind.Absolute, out var uri))
                return null;
            var builder = new UriBuilder(uri) { Scheme = scheme };
            return builder.ToString();
        }
    }
}
