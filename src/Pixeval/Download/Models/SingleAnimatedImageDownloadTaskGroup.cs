using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Misaki;
using Pixeval.Database;
using Pixeval.Options;
using Pixeval.Util;
using Pixeval.Util.IO;
using WinUI3Utilities;

namespace Pixeval.Download.Models;

public partial class SingleAnimatedImageDownloadTaskGroup : SingleImageDownloadTaskGroupBase
{
    public ISingleAnimatedImage Entry => DatabaseEntry.Entry.To<ISingleAnimatedImage>();

    private UgoiraDownloadFormat DestinationUgoiraFormat { get; }

    public SingleAnimatedImageDownloadTaskGroup(ISingleAnimatedImage entry, string destination) : base(entry, destination)
    {
        if (entry.PreferredAnimatedImageType is not SingleAnimatedImageType.SingleZipFile and not SingleAnimatedImageType.SingleFile)
            ThrowHelper.InvalidOperation($"{nameof(ISingleAnimatedImage.PreferredAnimatedImageType)} should be {nameof(SingleAnimatedImageType.SingleZipFile)} or {nameof(SingleAnimatedImageType.SingleFile)}");
        // DatabaseEntry.Destination可以包含未被替换的token，从此可以拿到UgoiraDownloadFormat.Original
        DestinationUgoiraFormat = IoHelper.GetUgoiraFormat(Path.GetExtension(DatabaseEntry.Destination));
    }

    public SingleAnimatedImageDownloadTaskGroup(DownloadHistoryEntry entry) : base(entry)
    {
        DestinationUgoiraFormat = IoHelper.GetUgoiraFormat(Path.GetExtension(DatabaseEntry.Destination));
    }

    protected override async Task AfterDownloadAsyncOverride(ImageDownloadTask sender, CancellationToken token = default)
    {
        if (DestinationUgoiraFormat is UgoiraDownloadFormat.Original)
            return;
        switch (Entry.PreferredAnimatedImageType)
        {
            case SingleAnimatedImageType.SingleZipFile:
            {
                await Entry.ZipImageDelays!.TryPreloadListAsync(Entry);
                var msDelays = Entry.ZipImageDelays!;
                var zipPath = Destination + ".zip";
                File.Move(Destination, zipPath);
                await using var read = FileHelper.OpenAsyncRead(zipPath);
                using var image = await read.UgoiraSaveToImageAsync(msDelays);
                image.SetIdTags(Entry);
                await image.UgoiraSaveToFileAsync(Destination, DestinationUgoiraFormat);
                break;
            }
            case SingleAnimatedImageType.SingleFile:
                await ExifManager.SetTagsAsync(Destination, Entry, DestinationUgoiraFormat, token);
                break;
        }
    }
}
