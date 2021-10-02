using System;
using System.Collections.Generic;
using System.Linq;
using Pixeval.Options;
using Pixeval.Util;

namespace Pixeval.Controls.Setting.UI.Model
{
    public record ApplicationThemeSettingEntryItem : IStringRepresentableItem
    {
        public static readonly IEnumerable<IStringRepresentableItem> AvailableItems = Enum.GetValues<ApplicationTheme>().Select(a => new ApplicationThemeSettingEntryItem(a));

        public ApplicationThemeSettingEntryItem(ApplicationTheme item)
        {
            Item = item;
            StringRepresentation = item.GetLocalizedResourceContent()!;
        }

        public object Item { get; }

        public string StringRepresentation { get; }
    }
}