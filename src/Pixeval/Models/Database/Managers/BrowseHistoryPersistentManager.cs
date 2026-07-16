// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Pixeval.Utilities;
using SQLite;

namespace Pixeval.Models.Database.Managers;

public sealed class BrowseHistoryPersistentManager(SQLiteConnection db, FileLogger logger)
    : WorkHistoryPersistentManager<BrowseHistoryEntry>(db, logger);
