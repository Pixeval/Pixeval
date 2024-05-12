#region Copyright
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2024 Pixeval/ProxyAppSettingsEntry.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using Pixeval.AppManagement;
using Pixeval.Controls.Settings;
using Pixeval.Options;

namespace Pixeval.Settings.Models;

public class ProxyAppSettingsEntry(AppSettings appSettings)
    : EnumAppSettingsEntry<ProxyType>(appSettings, t => t.ProxyType)
{
    public override ProxySettingsExpander Element => new() { Entry = this };

    public Action<string?>? ProxyChanged { get; set; }

    public string? Proxy
    {
        get => Settings.Proxy;
        set
        {
            if (Settings.Proxy != value)
                return;
            Settings.Proxy = value;
            OnPropertyChanged();
        }
    }

    public string? MakoProxy
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

    public override void ValueReset()
    {
        base.ValueReset();
        OnPropertyChanged(nameof(Proxy));
        ProxyChanged?.Invoke(MakoProxy);
    }
}
