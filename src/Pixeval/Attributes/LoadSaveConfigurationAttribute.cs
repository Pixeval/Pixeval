using System;

namespace Pixeval.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class LoadSaveConfigurationAttribute : Attribute
{
    public LoadSaveConfigurationAttribute(Type targetType, string containerName)
    {
        TargetType = targetType;
        ContainerName = containerName;
    }
    public Type TargetType { get; }
    public string ContainerName { get; }
    public string CastMethod { get; set; } = "null!";
}