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

public class IllustrationDownloadFormatSettingsEntry(AppSettings settings)
    : SingleValueSettingsEntry<AppSettings, object>(
        settings,
        nameof(AppSettings.IllustrationDownloadFormat),
        I18NManager.GetResource(EnumResources.WorkTypeEnumIllustration),
        "",
        Symbol.Image,
        null,
        static appSettings => new IllustrationDownloadFormatToken(appSettings.IllustrationDownloadFormat),
        static (appSettings, value) => appSettings.IllustrationDownloadFormat = ((IllustrationDownloadFormatToken) value).Value),
        IEnumSettingsEntry<object>
{
    public IReadOnlyList<IReadOnlyStringPair<object>> EnumItems { get; } = CreateEnumItems();

    private static IReadOnlyList<IReadOnlyStringPair<object>> CreateEnumItems()
    {
        var builtIns = SymbolComboBoxItem.GetValues<IllustrationDownloadFormat>()
            .Where(static t => (IllustrationDownloadFormat) t.Value is IllustrationDownloadFormat.Original)
            .Select(t => t with { Value = IllustrationDownloadFormatToken.BuiltIn((IllustrationDownloadFormat) t.Value) });

        var extensions = App.AppViewModel.AppServiceProvider.GetRequiredService<ExtensionService>()
            .ActiveStaticImageFormatProviders
            .Select(t => new SymbolComboBoxItem(IllustrationDownloadFormatToken.Extension(t), t.FormatDescription, t is IEntryExtension entry ? entry.Icon : Symbol.Image));

        return [.. builtIns.Concat(extensions)];
    }

    public override object Value
    {
        get
        {
            var value = base.Value;
            return EnumItems.Any(t => Equals(t.Value, value))
                ? value
                : IllustrationDownloadFormatToken.Default;
        }
        set => base.Value = value;
    }
}
