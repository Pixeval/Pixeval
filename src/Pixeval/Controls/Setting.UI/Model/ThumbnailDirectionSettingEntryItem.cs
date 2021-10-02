using System;
using System.Collections.Generic;
using System.Linq;
using Pixeval.Options;
using Pixeval.Util;

namespace Pixeval.Controls.Setting.UI.Model
{
    public record ThumbnailDirectionSettingEntryItem : IStringRepresentableItem
    {
        public static IEnumerable<IStringRepresentableItem> AvailableItems = Enum.GetValues<ThumbnailDirection>().Select(t => new ThumbnailDirectionSettingEntryItem(t));

        public ThumbnailDirectionSettingEntryItem(ThumbnailDirection item)
        {
            Item = item;
            StringRepresentation = item.GetLocalizedResourceContent()!;
        }

        public object Item { get; }

        public string StringRepresentation { get; }
    }
}