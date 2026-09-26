// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

namespace Pixeval.AppManagement.Settings;

public record DownloadFormatsSettings
{
    public string IllustrationDownloadFormat { get; set; } = Models.Download.IllustrationDownloadFormatToken.DefaultToken;

    public string UgoiraDownloadFormat { get; set; } = Models.Download.UgoiraDownloadFormatToken.DefaultToken;

    public string NovelDownloadFormat { get; set; } = Models.Download.NovelDownloadFormatToken.DefaultToken;
}
