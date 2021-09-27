using System;

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
}