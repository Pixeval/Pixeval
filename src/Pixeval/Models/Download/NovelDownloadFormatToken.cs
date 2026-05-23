// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Pixeval.Extensions.Common.FormatProviders;
using Pixeval.Models.Options;

namespace Pixeval.Models.Download;

public readonly record struct NovelDownloadFormatToken(string Value)
{
    public const string ExtensionPrefix = "extension:";

    public const string DefaultToken = nameof(NovelDownloadFormat.OriginalTxt);

    public const NovelDownloadFormat DefaultBuiltInFormat = NovelDownloadFormat.OriginalTxt;

    public static NovelDownloadFormatToken Default => BuiltIn(DefaultBuiltInFormat);

    public NovelDownloadFormat? BuiltInFormat => Enum.TryParse<NovelDownloadFormat>(Value, out var format) ? format : null;

    public string? ExtensionFormatExtension => Value.StartsWith(ExtensionPrefix, StringComparison.Ordinal)
        ? Value[ExtensionPrefix.Length..]
        : null;

    public bool IsExtension => ExtensionFormatExtension is not null;

    public static NovelDownloadFormatToken BuiltIn(NovelDownloadFormat format) => new(format.ToString());

    public static NovelDownloadFormatToken Extension(IFormatProviderExtension extension) => new(ExtensionPrefix + extension.FormatExtension);

    public static implicit operator string(NovelDownloadFormatToken token) => token.Value;

    public static implicit operator NovelDownloadFormatToken(string value) => new(value);

    public override string ToString() => Value;
}
