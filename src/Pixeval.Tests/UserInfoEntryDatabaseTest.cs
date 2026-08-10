// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixeval.Models.Database;
using Pixeval.Models.Database.Managers;
using SQLite;

namespace Pixeval.Tests;

[TestClass]
public sealed class UserInfoEntryDatabaseTest
{
    [TestMethod]
    public void DerivedEntries_KeepIndependentIdIndexes()
    {
        using var db = new SQLiteConnection(":memory:");
        _ = new WorkSubscriptionPersistentManager(db);
        _ = new BlockedUserPersistentManager(db);

        var subscriptionId = db.GetMapping<WorkSubscriptionEntry>().Columns
            .Single(column => column.Name is nameof(WorkSubscriptionEntry.Id));
        var blockedUserId = db.GetMapping<BlockedUserEntry>().Columns
            .Single(column => column.Name is nameof(BlockedUserEntry.Id));

        Assert.AreEqual(typeof(WorkSubscriptionEntry), subscriptionId.PropertyInfo?.DeclaringType);
        Assert.AreEqual(typeof(BlockedUserEntry), blockedUserId.PropertyInfo?.DeclaringType);
        var subscriptionIndexes = db.Query<IndexRow>(
                $"pragma index_list(\"{nameof(WorkSubscriptionEntry)}\")")
            .Select(static row => row.Name)
            .ToArray();
        var blockedUserIndexes = db.Query<IndexRow>(
                $"pragma index_list(\"{nameof(BlockedUserEntry)}\")")
            .Select(static row => row.Name)
            .ToArray();

        Assert.AreSequenceEqual(["IX_WorkSubscriptionEntry_Key"], subscriptionIndexes, SequenceOrder.InAnyOrder);
        Assert.AreSequenceEqual(["IX_BlockedUserEntry_Id"], blockedUserIndexes, SequenceOrder.InAnyOrder);

        var subscriptionIndexColumns = db.Query<IndexColumnRow>(
                "pragma index_info(\"IX_WorkSubscriptionEntry_Key\")")
            .OrderBy(static row => row.Seq)
            .Select(static row => row.Name)
            .ToArray();
        var blockedUserIndexColumns = db.Query<IndexColumnRow>(
                "pragma index_info(\"IX_BlockedUserEntry_Id\")")
            .OrderBy(static row => row.Seq)
            .Select(static row => row.Name)
            .ToArray();

        Assert.AreSequenceEqual(
        [
            nameof(WorkSubscriptionEntry.Id),
                nameof(WorkSubscriptionEntry.SubscriptionType),
                nameof(WorkSubscriptionEntry.WorkKind)
        ], subscriptionIndexColumns);
        Assert.AreSequenceEqual([nameof(BlockedUserEntry.Id)], blockedUserIndexColumns);
    }

    private sealed class IndexRow
    {
        public string Name { get; set; } = "";
    }

    private sealed class IndexColumnRow
    {
        public int Seq { get; set; }

        public string Name { get; set; } = "";
    }
}
