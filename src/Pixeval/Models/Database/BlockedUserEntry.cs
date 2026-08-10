// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using SQLite;

namespace Pixeval.Models.Database;

public sealed class BlockedUserEntry : UserInfoEntry
{
    [Indexed("IX_BlockedUserEntry_Id", 0, Unique = true)]
    public override long Id { get; set; }

    public void UpdateFrom(BlockedUserEntry entry) => UpdateUserInfoFrom(entry);
}
