// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Misaki;
using Pixeval.Extensions.Common.FormatProviders;
using Pixeval.Models.Database;
using Pixeval.Models.Extensions;
using Pixeval.Models.Options;
using Pixeval.Utilities;
using Pixeval.Utilities.IO;

namespace Pixeval.Models.Download.Tasks;

public class SingleAnimatedImageDownloadTaskGroup : SingleImageDownloadTaskGroupBase
{
    public ISingleAnimatedImage Entry => (ISingleAnimatedImage) DatabaseEntry.Entry;

    private UgoiraDownloadFormatToken DestinationUgoiraFormat { get; }

    public SingleAnimatedImageDownloadTaskGroup(ISingleAnimatedImage entry, string destination) : base(entry, destination)
    {
        if (entry.PreferredAnimatedImageType is not SingleAnimatedImageType.SingleZipFile and not SingleAnimatedImageType.SingleFile)
            throw new InvalidOperationException($"{nameof(ISingleAnimatedImage.PreferredAnimatedImageType)} should be {nameof(SingleAnimatedImageType.SingleZipFile)} or {nameof(SingleAnimatedImageType.SingleFile)}");
        DestinationUgoiraFormat = IoHelper.GetAvailableUgoiraDownloadFormatToken();
        DatabaseEntry.FormatToken = DestinationUgoiraFormat.Value;
    }

    public SingleAnimatedImageDownloadTaskGroup(DownloadHistoryEntry entry) : base(entry)
    {
        DestinationUgoiraFormat = GetFormatToken(entry);
    }

    protected override async Task AfterDownloadAsyncOverride(ImageDownloadTask sender, CancellationToken token = default)
    {
        if (DestinationUgoiraFormat.ExtensionFormatExtension is { } extension)
        {
            await FormatByExtensionAsync(sender, extension);
            return;
        }

        var builtInFormat = DestinationUgoiraFormat.BuiltInFormat ?? UgoiraDownloadFormatToken.DefaultBuiltInFormat;
        if (builtInFormat is UgoiraDownloadFormat.Original)
            return;

        throw new NotSupportedException(builtInFormat.ToString());
    }

    private async Task FormatByExtensionAsync(ImageDownloadTask sender, string extension)
    {
        var provider = GetExtensionService().GetAnimatedImageFormatProvider(extension)
            ?? throw new NotSupportedException(extension);
        var tempPath = sender.Destination + ".source";
        FileHelper.Move(sender.Destination, tempPath, true);
        IReadOnlyDictionary<Stream, int>? streams = null;
        try
        {
            switch (Entry.PreferredAnimatedImageType)
            {
                case SingleAnimatedImageType.SingleZipFile:
                    await Entry.ZipImageDelays!.TryPreloadListAsync(Entry);
                    await using (var read = File.OpenAsyncRead(tempPath))
                        streams = (await Streams.ReadZipAsync(read, true))
                            .ToArray<Stream>()
                            .Zip(Entry.ZipImageDelays!)
                            .ToDictionary(t => t.First, t => t.Second);
                    break;
                case SingleAnimatedImageType.SingleFile:
                    await using (var read = File.OpenAsyncRead(tempPath))
                        streams = await IoHelper.SplitAnimatedImageStreamAsync(read);
                    break;
                default:
                    throw new NotSupportedException(Entry.PreferredAnimatedImageType.ToString());
            }

            await provider.FormatImageAsync(streams, sender.Destination);
        }
        finally
        {
            if (streams is not null)
                foreach (var stream in streams.Keys)
                    await stream.DisposeAsync();
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    private static UgoiraDownloadFormatToken GetFormatToken(DownloadHistoryEntry entry)
    {
        if (!string.IsNullOrWhiteSpace(entry.FormatToken))
            return IoHelper.GetAvailableUgoiraDownloadFormatToken(entry.FormatToken);

        return UgoiraDownloadFormatToken.Default;
    }

    private static ExtensionService GetExtensionService() =>
        App.AppViewModel.AppServiceProvider.GetRequiredService<ExtensionService>();
}
