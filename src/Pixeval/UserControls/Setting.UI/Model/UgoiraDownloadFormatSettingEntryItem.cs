using Pixeval.Options;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Pixeval.UserControls.Setting.UI.Model;

public record UgoiraDownloadFormatSettingEntryItem : StringRepresentableItem, IAvailableItems
{
    public UgoiraDownloadFormatSettingEntryItem(UgoiraDownloadFormat item) : base(item)
    {
    }

    public static IEnumerable<StringRepresentableItem> AvailableItems { get; } = Enum.GetValues<UgoiraDownloadFormat>().Select(a => new UgoiraDownloadFormatSettingEntryItem(a));
}
