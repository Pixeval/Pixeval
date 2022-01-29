using System;

namespace Pixeval.Attributes;

/// <summary>
/// 根据设置类属性生成ViewModel属性（可用<see cref="SettingsViewModelExclusionAttribute"/>类，在设置类里指定不生成属性的例外）
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class SettingsViewModelAttribute : Attribute
{
    public SettingsViewModelAttribute(Type settingType, string settingName)
    {
        SettingType = settingType;
        SettingName = settingName;
    }
    public Type SettingType { get; }

    public string SettingName { get; }
}