// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Misaki;

namespace Pixeval.ViewModels;

public abstract class ThumbnailEntryViewModel<T>(T entry) : EntryViewModel<T>(entry)
    where T : class, IIdentityInfo
{
    public string Id => Entry.Id;

    public abstract string? ThumbnailUrl { get; }

    public override bool Equals(object? obj) => obj is ThumbnailEntryViewModel<T> viewModel && Entry.Equals(viewModel.Entry);

    public override int GetHashCode() => Entry.GetHashCode();
}
