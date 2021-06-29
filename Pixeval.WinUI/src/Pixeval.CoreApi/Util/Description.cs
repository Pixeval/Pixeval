using System;
using System.Reflection;
using JetBrains.Annotations;

namespace Pixeval.CoreApi.Util
{
    [AttributeUsage(AttributeTargets.Field)]
    [PublicAPI]
    public class Description : Attribute
    {
        public string Name { get; }

        public Description(string name)
        {
            Name = name;
        }
    }

    [PublicAPI]
    public static class DescriptionHelper
    {
        public static string GetDescription<TEnum>(this TEnum @enum) where TEnum : Enum
        {
            return (typeof(TEnum).GetField(@enum.ToString())?.GetCustomAttribute(typeof(Description)) as Description)?.Name ?? throw new InvalidOperationException("Attribute not found");
        }
    }
}