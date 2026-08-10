// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using CommunityToolkit.Mvvm.ComponentModel;
using Mako.Model;
using Pixeval.Models.Database;
using Pixeval.Utilities;

namespace Pixeval.ViewModels.Settings;

public sealed partial class BlockedUserItemViewModel(BlockedUserEntry entry) : ViewModelBase
{
    public BlockedUserEntry Entry { get; } = entry;

    [ObservableProperty]
    public partial UserBasicInfo User { get; private set; } = BlockedContentModelHelper.CreateBlockedUserPreview(entry);

    internal void UpdateUser(BlockedUserEntry entry)
    {
        Entry.UpdateFrom(entry);
        User = BlockedContentModelHelper.CreateBlockedUserPreview(Entry);
    }
}
