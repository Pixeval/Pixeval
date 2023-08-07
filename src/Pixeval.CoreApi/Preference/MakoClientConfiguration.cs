#region Copyright (c) Pixeval/Pixeval.CoreApi
// GPL v3 License
// 
// Pixeval/Pixeval.CoreApi
// Copyright (c) 2021 Pixeval.CoreApi/MakoClientConfiguration.cs
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

using System.Globalization;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Pixeval.CoreApi.Preference;

/// <summary>
///     Contains all the user-configurable keys
/// </summary>
[PublicAPI]
public record MakoClientConfiguration(int ConnectionTimeout, bool Bypass, string? MirrorHost, CultureInfo CultureInfo)
{
    public MakoClientConfiguration() : this(5000, false, string.Empty, CultureInfo.CurrentCulture)
    {
    }

    [JsonIgnore]
    public CultureInfo CultureInfo { get; set; } = CultureInfo;

    [JsonPropertyName("connectionTimeout")]
    public int ConnectionTimeout { get; set; } = ConnectionTimeout;

    /// <summary>
    ///     Automatically bypass GFW or not, default is set to true.
    ///     If you are currently living in China Mainland, turn it on to make sure
    ///     you can use Mako without using any kind of proxy, otherwise you will
    ///     need a proper proxy server to bypass the GFW
    /// </summary>
    [JsonPropertyName("bypass")]
    public bool Bypass { get; set; } = Bypass;

    /// <summary>
    ///     Mirror server's host of image downloading
    /// </summary>
    [JsonPropertyName("mirrorHost")]
    public string? MirrorHost { get; set; } = MirrorHost;
}