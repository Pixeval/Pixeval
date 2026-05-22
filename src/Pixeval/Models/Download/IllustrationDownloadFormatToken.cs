// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using Pixeval.Extensions.Common.FormatProviders;
using Pixeval.Models.Options;

namespace Pixeval.Models.Download;

public readonly record struct IllustrationDownloadFormatToken(string Value)
{
    public const string ExtensionPrefix = "extension:";

    public const string DefaultToken = nameof(IllustrationDownloadFormat.Original);

    public const IllustrationDownloadFormat DefaultBuiltInFormat = IllustrationDownloadFormat.Original;

    public static IllustrationDownloadFormatToken Default => BuiltIn(DefaultBuiltInFormat);

    public IllustrationDownloadFormat? BuiltInFormat =>
        Enum.TryParse<IllustrationDownloadFormat>(Value, out var format) ? format : null;

    public string? ExtensionFormatExtension => Value.StartsWith(ExtensionPrefix, StringComparison.Ordinal)
        ? Value[ExtensionPrefix.Length..]
        : null;

    public bool IsExtension => ExtensionFormatExtension is not null;

    public static IllustrationDownloadFormatToken BuiltIn(IllustrationDownloadFormat format) => new(format.ToString());

    public static IllustrationDownloadFormatToken Extension(IFormatProviderExtension extension) =>
        new(ExtensionPrefix + extension.FormatExtension);

    public static implicit operator string(IllustrationDownloadFormatToken token) => token.Value;

    public static implicit operator IllustrationDownloadFormatToken(string value) => new(value);

    public override string ToString() => Value;
}
