// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Misaki;
using SQLite;

namespace Pixeval.Models.Database;

public sealed class DownloadHistoryEntry : DownloadHistoryEntryBase
{
    public DownloadHistoryEntry(string destination, IArtworkInfo entry) : base(destination, entry)
    {
    }

    public DownloadHistoryEntry()
    {
    }

    /// <inheritdoc />
    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
    [Indexed(Unique = true)]
    public override string Destination { get; init; } = null!;
}
