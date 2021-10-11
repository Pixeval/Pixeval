using System;
using System.Collections.Generic;
using System.Linq;
using Pixeval.CoreApi.Global.Enum;

namespace Pixeval.Controls.Setting.UI.Model
{
    public record SearchDurationSettingEntryItem : IStringRepresentableItem
    {
        public static readonly IEnumerable<IStringRepresentableItem> AvailableItems = Enum.GetValues<SearchDuration>().Select(s => new SearchDurationSettingEntryItem(s));

        public SearchDurationSettingEntryItem(SearchDuration item)
        {
            Item = item;
            StringRepresentation = item switch
            {
                SearchDuration.Undecided => MiscResources.SearchDurationUndecided,
                SearchDuration.WithinLastDay => MiscResources.SearchDurationWithinLastDay,
                SearchDuration.WithinLastWeek => MiscResources.SearchDurationWithinLastWeek,
                SearchDuration.WithinLastMonth => MiscResources.SearchDurationWithinLastMonth,
                _ => throw new ArgumentOutOfRangeException(nameof(item), item, null)
            };
        }

        public object Item { get; }
        public string StringRepresentation { get; }
    }
}