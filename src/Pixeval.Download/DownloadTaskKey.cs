// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

namespace Pixeval.Download;

public readonly record struct DownloadTaskKey
{
    private DownloadTaskKey(
        string destination,
        int workSubscriptionId,
        string? artworkId)
    {
        Destination = destination;
        WorkSubscriptionId = workSubscriptionId;
        ArtworkId = artworkId;
    }

    public string Destination { get; }

    public int WorkSubscriptionId { get; }

    public string? ArtworkId { get; }

    public static DownloadTaskKey CreateOrdinary(string destination) => new(destination, 0, null);

    public static DownloadTaskKey CreateSubscription(
        string destination,
        int workSubscriptionId,
        string artworkId) =>
        new(destination, workSubscriptionId, artworkId);
}
