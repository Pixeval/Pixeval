// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

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

public partial class SingleImageDownloadTaskGroup : SingleImageDownloadTaskGroupBase
{
    public ISingleImage Entry => DatabaseEntry.Entry.To<ISingleImage>();

    private IllustrationDownloadFormat DestinationIllustrationFormat { get; }

    public SingleImageDownloadTaskGroup(ISingleImage entry, string destination) : base(entry, destination)
    {
        // DatabaseEntry.Destination可以包含未被替换的token，从此可以拿到IllustrationDownloadFormat.Original
        DestinationIllustrationFormat = IoHelper.GetIllustrationFormat(Path.GetExtension(DatabaseEntry.Destination));
    }

    public SingleImageDownloadTaskGroup(DownloadHistoryEntry entry) : base(entry)
    {
        DestinationIllustrationFormat = IoHelper.GetIllustrationFormat(Path.GetExtension(DatabaseEntry.Destination));
    }

    protected override async Task AfterDownloadAsyncOverride(ImageDownloadTask sender, CancellationToken token = default)
    {
        if (DestinationIllustrationFormat is not IllustrationDownloadFormat.Original)
            await ExifManager.SetTagsAsync(Destination, Entry, DestinationIllustrationFormat, token);
    }
}
