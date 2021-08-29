using System.Collections.Generic;
using Mako.Global.Enum;
using Pixeval.Util;
using Pixeval.Util.Generic;

namespace Pixeval.Options
{
    public record TargetFilterWrapper : ILocalizedBox<TargetFilter, TargetFilterWrapper>
    {
        public TargetFilter Value { get; }

        public string LocalizedString { get; }

        public TargetFilterWrapper(TargetFilter value, string localizedString)
        {
            Value = value;
            LocalizedString = localizedString;
        }

        public static IEnumerable<TargetFilterWrapper> AvailableOptions()
        {
            return new TargetFilterWrapper[]
            {
                new(TargetFilter.ForAndroid, MiscResources.TargetFilterForAndroid),
                new(TargetFilter.ForIos, MiscResources.TargetFilterForIOS)
            };
        }
    }
}