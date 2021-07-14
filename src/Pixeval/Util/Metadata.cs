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

    public static class MetadataHelper
    {
        public static string? GetMetadataOnEnumMember(this Enum e)
        {
            return (e.GetType().GetField(e.ToString())?.GetCustomAttribute(typeof(Metadata), false) as Metadata)?.Key;
        }
    }
}