// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using Pixeval.Extensions.Common.FormatProviders;
using Pixeval.Models.Options;

namespace Pixeval.Models.Download;

public readonly record struct UgoiraDownloadFormatToken(string Value)
{
    public const string ExtensionPrefix = "extension:";

    public const string DefaultToken = nameof(UgoiraDownloadFormat.WebPLossless);

    public const UgoiraDownloadFormat DefaultBuiltInFormat = UgoiraDownloadFormat.WebPLossless;

    public static UgoiraDownloadFormatToken Default => BuiltIn(DefaultBuiltInFormat);

    public UgoiraDownloadFormat? BuiltInFormat =>
        Enum.TryParse<UgoiraDownloadFormat>(Value, out var format) ? format : null;

    public string? ExtensionFormatExtension => Value.StartsWith(ExtensionPrefix, StringComparison.Ordinal)
        ? Value[ExtensionPrefix.Length..]
        : null;

    public bool IsExtension => ExtensionFormatExtension is not null;

    public static UgoiraDownloadFormatToken BuiltIn(UgoiraDownloadFormat format) => new(format.ToString());

    public static UgoiraDownloadFormatToken Extension(IFormatProviderExtension extension) =>
        new(ExtensionPrefix + extension.FormatExtension);

    public static implicit operator string(UgoiraDownloadFormatToken token) => token.Value;

    public static implicit operator UgoiraDownloadFormatToken(string value) => new(value);

    public override string ToString() => Value;
}