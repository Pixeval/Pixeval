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

public class UgoiraDownloadFormatSettingsEntry(DownloadSettingsGroup settings)
    : SingleValueSettingsEntry<DownloadSettingsGroup, object>(
        settings,
        nameof(DownloadSettingsGroup.UgoiraDownloadFormat),
        I18NManager.GetResource(EnumResources.WorkTypeEnumUgoira),
        "",
        Symbol.Gif,
        null,
        static settings => new UgoiraDownloadFormatToken(settings.UgoiraDownloadFormat),
        static (settings, value) => settings.UgoiraDownloadFormat = ((UgoiraDownloadFormatToken) value).Value),
        IEnumSettingsEntry<object>
{
    public IReadOnlyList<IReadOnlyStringPair<object>> EnumItems { get; } = CreateEnumItems();

    private static IReadOnlyList<IReadOnlyStringPair<object>> CreateEnumItems()
    {
        var builtIns = SymbolComboBoxItem.GetValues<UgoiraDownloadFormat>()
            .Where(static t => (UgoiraDownloadFormat) t.Value is UgoiraDownloadFormat.Original)
            .Select(t => new SymbolComboBoxItem(UgoiraDownloadFormatToken.BuiltIn((UgoiraDownloadFormat) t.Value), t.Description, t.Symbol));

        var extensions = App.AppViewModel.AppServiceProvider.GetRequiredService<ExtensionService>()
            .ActiveAnimatedImageFormatProviders
            .Select(t => new SymbolComboBoxItem(UgoiraDownloadFormatToken.Extension(t), t.FormatDescription, t is IEntryExtension entry ? entry.Icon : Symbol.Gif));

        return [.. builtIns.Concat(extensions)];
    }

    public override object Value
    {
        get
        {
            var value = base.Value;
            return EnumItems.Any(t => Equals(t.Value, value))
                ? value
                : UgoiraDownloadFormatToken.Default;
        }
        set => base.Value = value;
    }
}
