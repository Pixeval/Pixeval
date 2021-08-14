using System.Collections.Generic;
using Mako.Global.Enum;
using Pixeval.Util;

namespace Pixeval.Options
{
    public record TargetFilterWrapper : ILocalizedBox<TargetFilter>
    {
        public static readonly IEnumerable<TargetFilterWrapper> Available = new TargetFilterWrapper[]
        {
            new(TargetFilter.ForAndroid, MiscResources.TargetFilterForAndroid),
            new(TargetFilter.ForIos, MiscResources.TargetFilterForIOS)
        };

        public TargetFilter Value { get; }

        public string LocalizedString { get; }

        public TargetFilterWrapper(TargetFilter value, string localizedString)
        {
            Value = value;
            LocalizedString = localizedString;
        }
    }
}