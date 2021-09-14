using System;
using System.Reflection;

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

    [AttributeUsage(AttributeTargets.Field)]
    public class LocalizedResource : Attribute
    {
        public Type ResourceLoader { get; }

        public string Key { get; }
        public object? FormatKey { get; }

        public LocalizedResource(Type resourceLoader, string key, object? formatKey = null)
        {
            ResourceLoader = resourceLoader;
            Key = key;
            FormatKey = formatKey;
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