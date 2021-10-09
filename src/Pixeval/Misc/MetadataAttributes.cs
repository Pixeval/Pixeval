using System;
using Pixeval.Util;

namespace Pixeval.Misc
{
    [AttributeUsage(AttributeTargets.Field)]
    public class Metadata : Attribute
    {
        public Metadata(string key)
        {
            Key = key;
        }

        public string Key { get; }
    }

    public static class MetadataAttributeHelper
    {
        public static string? GetMetadataOnEnumMember(this Enum e)
        {
            return e.GetCustomAttribute<Metadata>()?.Key;
        }
    }
}