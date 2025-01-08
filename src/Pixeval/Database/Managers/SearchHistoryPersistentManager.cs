// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using LiteDB;
using Microsoft.Extensions.DependencyInjection;

namespace Pixeval.Database.Managers;

public class SearchHistoryPersistentManager(ILiteDatabase db, int maximumRecords)
    : SimplePersistentManager<SearchHistoryEntry>(db, maximumRecords)
{
    public static void AddHistory(string text, string? translatedName = null)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;
        var manager = App.AppViewModel.AppServiceProvider.GetRequiredService<SearchHistoryPersistentManager>();
        _ = manager.TryDelete(x => x.Value == text);
        var searchHistoryEntry = new SearchHistoryEntry
        {
            Value = text,
            TranslatedName = translatedName,
            Time = DateTime.Now
        };
        manager.Insert(searchHistoryEntry);
    }
}
