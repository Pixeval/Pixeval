using Pixeval.Options;
using System.Collections.Generic;
using System;
using System.Linq;
using Pixeval.Controls;
using Pixeval.Attributes;

namespace Pixeval.SettingsModels;

public record UgoiraDownloadFormatSettingEntryItem : StringRepresentableItem, IAvailableItems
{
    public UgoiraDownloadFormatSettingEntryItem(UgoiraDownloadFormat item) : base(item, item.GetLocalizedResourceContent()!)
    {
    }

    public static IEnumerable<StringRepresentableItem> AvailableItems { get; } = Enum.GetValues<UgoiraDownloadFormat>().Select(a => new UgoiraDownloadFormatSettingEntryItem(a));
}
