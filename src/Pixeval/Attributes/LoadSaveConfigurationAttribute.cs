using System;

namespace Pixeval.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class LoadSaveConfigurationAttribute : Attribute
{
    public LoadSaveConfigurationAttribute(Type targetType)
    {
        TargetType = targetType;
    }
    public Type TargetType { get; }
}