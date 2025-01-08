// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Linq;
using LiteDB;
using Microsoft.Extensions.DependencyInjection;

namespace Pixeval.Database.Managers;

public class SearchHistoryPersistentManager(ILiteDatabase db, int maximumRecords)
    : SimplePersistentManager<SearchHistoryEntry>(db, maximumRecords)
{
    public static void AddHistory(string text, string? optTranslatedName = null)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;
        var manager = App.AppViewModel.AppServiceProvider.GetRequiredService<SearchHistoryPersistentManager>();
        if (manager.Count is 0 || manager.SelectLast(1).FirstOrDefault() is { Value: var last } && last != text)
        {
            manager.Insert(new SearchHistoryEntry
            {
                Value = text,
                TranslatedName = optTranslatedName,
                Time = DateTime.Now
            });
        }
    }
}
