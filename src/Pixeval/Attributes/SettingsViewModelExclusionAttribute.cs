using System;

namespace Pixeval.Attributes;

/// <summary>
/// <see cref="SettingsViewModelAttribute"/>类不生成属性的例外，放在设置类属性上
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class SettingsViewModelExclusionAttribute : Attribute
{

}