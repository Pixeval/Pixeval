// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Misaki;
using SQLite;

namespace Pixeval.Models.Database;

public sealed class SubscriptionDownloadHistoryEntry : DownloadHistoryEntryBase
{
    private const string IdentityIndex = "IX_SubscriptionDownloadHistoryEntry_Identity";

    public SubscriptionDownloadHistoryEntry(
        string destination,
        IArtworkInfo entry,
        int workSubscriptionId) : base(destination, entry)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(workSubscriptionId);
        if (string.IsNullOrWhiteSpace(entry.Id))
            throw new ArgumentException("The artwork ID cannot be empty.", nameof(entry));

        WorkSubscriptionId = workSubscriptionId;
        ArtworkId = entry.Id;
    }

    public SubscriptionDownloadHistoryEntry()
    {
    }

    [Indexed(IdentityIndex, 0, Unique = true)]
    public int WorkSubscriptionId { get; init; }

    [Indexed(IdentityIndex, 1, Unique = true)]
    public string ArtworkId { get; init; } = null!;

    /// <inheritdoc />
    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
    [Indexed(IdentityIndex, 2, Unique = true)]
    public override string Destination { get; init; } = null!;
}
