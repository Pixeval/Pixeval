// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using LiteDB;

namespace Pixeval.Database;

public class SearchHistoryEntry : IHistoryEntry
{
    [BsonId(true)]
    public ObjectId? HistoryEntryId { get; set; }

    /// <summary>
    /// Search value
    /// </summary>
    public string? Value { get; set; }

    public string? TranslatedName { get; set; }

    public DateTime Time { get; set; }
}
