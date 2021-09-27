using System;
using System.Reflection;
using Pixeval.Misc;

namespace Pixeval.Util
{
    public static class AttributeHelper
    {
        public static TAttribute? GetCustomAttribute<TAttribute>(this Enum e) where TAttribute : Attribute
        {
            return e.GetType().GetField(e.ToString())?.GetCustomAttribute(typeof(TAttribute), false) as TAttribute;
        }

        public static string? GetMetadataOnEnumMember(this Enum e)
        {
            return e.GetCustomAttribute<Metadata>()?.Key;
        }

        public static string? GetLocalizedResourceContent(this Enum e)
        {
            var attribute = e.GetCustomAttribute<LocalizedResource>();
            return attribute?.GetLocalizedResourceContent();
        }

        public static LocalizedResource? GetLocalizedResource(this Enum e)
        {
            return e.GetCustomAttribute<LocalizedResource>();
        }

        public static string? GetLocalizedResourceContent(this LocalizedResource attribute)
        {
            return attribute.ResourceLoader.GetField(attribute.Key)?.GetValue(null) as string;
        }
    }
}