// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
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
        "",
        "",
        Symbol.BookOpen,
        null,
        static settings => new NovelDownloadFormatToken(settings.NovelDownloadFormat),
        static (settings, value) => settings.NovelDownloadFormat = ((NovelDownloadFormatToken) value).Value),
        IEnumSettingsEntry<object>
{
    public NovelDownloadFormatSettingsEntry(AppSettings settings, WorkTypeEnum workType) : this(settings)
    {
        Description = "";
        (Icon, var header) = workType switch
        {
            WorkTypeEnum.Illustration => (Symbol.Image, EnumResources.WorkTypeIllustration),
            WorkTypeEnum.Manga => (Symbol.ImageMultiple, EnumResources.WorkTypeManga),
            WorkTypeEnum.Ugoira => (Symbol.Gif, EnumResources.WorkTypeEnumUgoira),
            WorkTypeEnum.Novel => (Symbol.BookOpen, EnumResources.WorkTypeNovel),
            _ => throw new ArgumentOutOfRangeException(nameof(workType))
        };
        Header = I18NManager.GetResource(header);
    }

    public IReadOnlyList<IReadOnlyStringPair<object>> EnumItems { get; } = CreateEnumItems();

    private static IReadOnlyList<IReadOnlyStringPair<object>> CreateEnumItems()
    {
        var builtIns = SymbolComboBoxItem.GetValues<NovelDownloadFormat>()
            .Select(t => new SymbolComboBoxItem(NovelDownloadFormatToken.BuiltIn((NovelDownloadFormat) t.Value), t.Description, t.Symbol));

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
