using System;
using System.Collections.Generic;
using System.Linq;
using Pixeval.Misc;
using Pixeval.Options;

namespace Pixeval.Controls.Setting.UI.Model
{
    public record MainPageTabItemSettingEntryItem : IStringRepresentableItem
    {
        public static readonly IEnumerable<IStringRepresentableItem> AvailableItems = Enum.GetValues<MainPageTabItem>().Select(m => new MainPageTabItemSettingEntryItem(m));

        public MainPageTabItemSettingEntryItem(MainPageTabItem item)
        {
            Item = item;
            StringRepresentation = item.GetLocalizedResourceContent()!;
        }

        public object Item { get; }

        public string StringRepresentation { get; }
    }
}