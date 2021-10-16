using System;
using System.Reflection;

namespace Pixeval.Misc
{
    public class DefaultValue : Attribute
    {
        public object? Value { get; }

        public Type? ValueFactoryType { get; }

        public DefaultValue(object? value)
        {
            Value = value;
        }

        /// <summary>
        /// Create a <see cref="DefaultValue"/> using a factory type, the type will be instantiated to provide the value
        /// </summary>
        /// <param name="valueFactoryType"></param>
        public DefaultValue(Type? valueFactoryType)
        {
            ValueFactoryType = valueFactoryType;
        }
    }

    public static class DefaultValueAttributeHelper
    {
        public static object? GetDefaultValue(this PropertyInfo propInfo)
        {
            if (propInfo.GetCustomAttribute<DefaultValue>() is { } attribute)
            {
                if (attribute.ValueFactoryType is { } type)
                {
                    return type.IsAssignableTo(typeof(IDefaultValueProvider))
                        ? ((IDefaultValueProvider) Activator.CreateInstance(type)!).ProvideValue()
                        : Activator.CreateInstance(type);
                }
                else
                {
                    return attribute.Value;
                }
            }

            return null;
        }

        public static void Initialize(object obj)
        {
            foreach (var propertyInfo in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                propertyInfo.SetValue(obj, propertyInfo.GetDefaultValue());
            }
        }
    }
}