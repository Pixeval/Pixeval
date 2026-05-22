// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Misaki;
using Pixeval.Extensions.Common.FormatProviders;
using Pixeval.Models.Database;
using Pixeval.Models.Extensions;
using Pixeval.Utilities;
using Pixeval.Utilities.IO;

namespace Pixeval.Models.Download.Tasks;

public class SingleImageDownloadTaskGroup : SingleImageDownloadTaskGroupBase
{
    public ISingleImage Entry => (ISingleImage) DatabaseEntry.Entry;

    private IllustrationDownloadFormatToken DestinationIllustrationFormat { get; }

    public SingleImageDownloadTaskGroup(ISingleImage entry, string destination) : base(entry, destination)
    {
        DestinationIllustrationFormat = IoHelper.GetAvailableIllustrationDownloadFormatToken();
        DatabaseEntry.FormatToken = DestinationIllustrationFormat.Value;
    }

    public SingleImageDownloadTaskGroup(DownloadHistoryEntry entry) : base(entry)
    {
        DestinationIllustrationFormat = GetFormatToken(entry);
    }

    protected override async Task AfterDownloadAsyncOverride(ImageDownloadTask sender, CancellationToken token = default)
    {
        if (DestinationIllustrationFormat.ExtensionFormatExtension is not { } extension)
            return;

        var provider = GetExtensionService().GetStaticImageFormatProvider(extension)
            ?? throw new NotSupportedException(extension);
        var tempPath = sender.Destination + ".source";
        FileHelper.Move(sender.Destination, tempPath, true);
        try
        {
            await using var stream = File.OpenAsyncRead(tempPath);
            await provider.FormatImageAsync(stream, sender.Destination);
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    private static IllustrationDownloadFormatToken GetFormatToken(DownloadHistoryEntry entry)
    {
        if (!string.IsNullOrWhiteSpace(entry.FormatToken))
            return IoHelper.GetAvailableIllustrationDownloadFormatToken(entry.FormatToken);

        var extension = Path.GetExtension(entry.Destination);
        return IoHelper.TryGetIllustrationFormat(extension, out var format)
            ? IllustrationDownloadFormatToken.BuiltIn(format)
            : IllustrationDownloadFormatToken.ExtensionPrefix + extension;
    }

    private static ExtensionService GetExtensionService() =>
        App.AppViewModel.AppServiceProvider.GetRequiredService<ExtensionService>();
}
