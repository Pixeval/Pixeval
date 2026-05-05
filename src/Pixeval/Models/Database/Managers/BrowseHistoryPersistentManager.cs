// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using SQLite;

namespace Pixeval.Models.Database.Managers;

public class BrowseHistoryPersistentManager(SQLiteConnection db)
    : SimplePersistentManager<BrowseHistoryEntry>(db, App.AppViewModel.AppSettings.MaximumBrowseHistoryRecords);
