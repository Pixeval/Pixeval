// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Collections.Generic;
using System.Linq;
using AutoSettingsPage;
using AutoSettingsPage.Models;
using FluentIcons.Common;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.AppManagement;
using Pixeval.Controls;
using Pixeval.Extensions.Common;
using Pixeval.Extensions.Common.FormatProviders;
using Pixeval.I18N;
using Pixeval.Models.Download;
using Pixeval.Models.Extensions;
using Pixeval.Models.Options;

namespace Pixeval.Models.Settings.Entries;

public class NovelDownloadFormatSettingsEntry(AppSettings settings)
    : SingleValueSettingsEntry<AppSettings, object>(
        settings,
        nameof(AppSettings.NovelDownloadFormat),
        I18NManager.GetResource(EnumResources.WorkTypeEnumNovel),
        "",
        Symbol.BookOpen,
        null,
        static settings => new NovelDownloadFormatToken(settings.NovelDownloadFormat),
        static (settings, value) => settings.NovelDownloadFormat = ((NovelDownloadFormatToken) value).Value),
        IEnumSettingsEntry<object>
{
    public IReadOnlyList<IReadOnlyStringPair<object>> EnumItems { get; } = CreateEnumItems();

    private static IReadOnlyList<IReadOnlyStringPair<object>> CreateEnumItems()
    {
        var builtIns = SymbolComboBoxItem.GetValues<NovelDownloadFormat>()
            .Select(t => t with { Value = NovelDownloadFormatToken.BuiltIn((NovelDownloadFormat) t.Value) });

        var extensions = App.AppViewModel.AppServiceProvider.GetRequiredService<ExtensionService>()
            .ActiveNovelFormatProviders
            .Select(t => new SymbolComboBoxItem(NovelDownloadFormatToken.Extension(t), t.FormatDescription, t is IEntryExtension entry ? entry.Icon : Symbol.Document));

        return [.. builtIns.Concat(extensions)];
    }

    public override object Value
    {
        get
        {
            var value = base.Value;
            return EnumItems.Any(t => Equals(t.Value, value))
                ? value
                : NovelDownloadFormatToken.Default;
        }
        set => base.Value = value;
    }
}
