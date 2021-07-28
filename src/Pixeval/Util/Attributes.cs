using System;
using System.Reflection;

namespace Pixeval.Util
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

    [AttributeUsage(AttributeTargets.Field)]
    public class LocalizedResource : Attribute
    {
        public Type ResourceLoader { get; set; }

        public string Key { get; set; }

        public LocalizedResource(Type resourceLoader, string key)
        {
            ResourceLoader = resourceLoader;
            Key = key;
        }
    }

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

        public static string? GetLocalizedResources(this Enum e)
        {
            var attribute = e.GetCustomAttribute<LocalizedResource>();
            return attribute?.ResourceLoader?.GetField(attribute?.Key ?? string.Empty)?.GetValue(null) as string;
        }
    }
}