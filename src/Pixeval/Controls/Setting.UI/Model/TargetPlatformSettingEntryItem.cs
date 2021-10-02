using System;
using System.Collections.Generic;
using System.Linq;
using Pixeval.CoreApi.Global.Enum;

namespace Pixeval.Controls.Setting.UI.Model
{
    public record TargetPlatformSettingEntryItem : IStringRepresentableItem
    {
        public static readonly IEnumerable<IStringRepresentableItem> AvailableItems = Enum.GetValues<TargetFilter>().Select(t => new TargetPlatformSettingEntryItem(t));

        public TargetPlatformSettingEntryItem(TargetFilter item)
        {
            Item = item;
            StringRepresentation = item switch
            {
                TargetFilter.ForAndroid => MiscResources.TargetFilterForAndroid,
                TargetFilter.ForIos => MiscResources.TargetFilterForIOS,
                _ => throw new ArgumentOutOfRangeException(nameof(item), item, null)
            };
        }

        public object Item { get; }

        public string StringRepresentation { get; }
    }
}