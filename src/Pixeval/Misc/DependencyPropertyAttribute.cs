using System;

namespace Pixeval.Misc;

/// <summary>
/// 生成如下代码
/// <code>
/// public static readonly DependencyProperty Property = DependencyProperty.Register("Field", typeof(Type), typeof(TClass), new PropertyMetadata(DefaultValue, OnPropertyChanged));
/// public Type Field { get => (Type)GetValue(Property); set => SetValue(Property, value); }
/// </code>
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class DependencyPropertyAttribute : Attribute
{
    public DependencyPropertyAttribute(string name, Type type)
    {
        Name = name;
        Type = type;
        IsSetterPublic = true;
        IsNullable = true;
    }

    public string Name { get; }

    public Type Type { get; }

    public bool IsSetterPublic { get; init; }

    public bool IsNullable { get; init; }

    public string DefaultValue { get; init; } = "DependencyProperty.UnsetValue";

    public bool InstanceChangedCallback { get; init; }
}