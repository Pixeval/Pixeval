// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Collections.Generic;
using System.Linq;
using AutoSettingsPage.Models;
using Pixeval.AppManagement;
using Pixeval.Controls;
using Pixeval.Download.MacroParser;
using Pixeval.Models.Download;

namespace Pixeval.Models.Settings.Entries;

public class DownloadMacroAppSettingsEntry(
    AppSettings settings)
    : StringSettingsEntry<AppSettings>(settings, t => t.DownloadPathMacro)
{
    public static ICollection<SymbolComboBoxItem> AvailableTransducerMacros { get; } =
    [
        ..DownloadPathMacroParser.MacroProvider
            .Where(m => m is ITransducer)
            .Select(m => new SymbolComboBoxItem($"@{{{m.Name}}}", m.Description, default))
    ];

    public static ICollection<SymbolComboBoxItem> AvailablePredicateMacros { get; } =
    [
        ..DownloadPathMacroParser.MacroProvider
            .Where(m => m is IPredicate)
            .Select(m => new SymbolComboBoxItem($"@{{{m.Name}?:}}", m.Description, default))
    ];
}
