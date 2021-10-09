using System;
using System.Reflection;

namespace Pixeval.Util
{
    public static class AttributeHelper
    {
        public static TAttribute? GetCustomAttribute<TAttribute>(this Enum e) where TAttribute : Attribute
        {
            return e.GetType().GetField(e.ToString())?.GetCustomAttribute(typeof(TAttribute), false) as TAttribute;
        }
    }
}