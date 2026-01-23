// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Misaki;
using Pixeval.Models.Database;
using Pixeval.Models.Options;
using Pixeval.Utilities.IO;

namespace Pixeval.Models.Download.Tasks;

public class SingleImageDownloadTaskGroup : SingleImageDownloadTaskGroupBase
{
    public ISingleImage Entry => (ISingleImage) DatabaseEntry.Entry;

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

    protected override Task AfterDownloadAsyncOverride(ImageDownloadTask sender, CancellationToken token = default)
    {
        return Task.CompletedTask;
    }
}
