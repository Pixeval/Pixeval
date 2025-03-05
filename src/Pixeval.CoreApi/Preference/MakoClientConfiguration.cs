// Copyright (c) Pixeval.CoreApi.
// Licensed under the GPL v3 License.

using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;

namespace Pixeval.CoreApi.Preference;

/// <summary>
/// Contains all the user-configurable keys
/// </summary>
public record MakoClientConfiguration(
    int ConnectionTimeout,
    bool DomainFronting,
    string? Proxy,
    string? Cookie,
    string? MirrorHost,
    CultureInfo CultureInfo)
{
    public MakoClientConfiguration() : this(5000, false, "", "", "", CultureInfo.CurrentCulture) { }

    [JsonIgnore] public CultureInfo CultureInfo { get; set; } = CultureInfo;

    [JsonPropertyName("userAgent")]
    public ProductInfoHeaderValue[] UserAgent { get; set; } =
    [
        new("Mozilla", "5.0"),
        new("(Windows NT 10.0; Win64; x64)"),
        new("AppleWebKit", "537.36"),
        new("(KHTML, like Gecko)"),
        new("Chrome", "133.0.0.0"),
        new("Safari", "537.36"),
        new("Edg", "133.0.0.0")
    ];

    [JsonPropertyName("connectionTimeout")]
    public int ConnectionTimeout { get; set; } = ConnectionTimeout;

    [JsonPropertyName("domainFronting")]
    public bool DomainFronting { get; set; } = DomainFronting;

    [JsonPropertyName("proxy")]
    public string? Proxy { get; set; } = Proxy;

    [JsonPropertyName("cookie")]
    public string? Cookie { get; set; } = Cookie;

    /// <summary>
    /// Mirror server's host of image downloading
    /// </summary>
    [JsonPropertyName("mirrorHost")]
    public string? MirrorHost { get; set; } = MirrorHost;
}
