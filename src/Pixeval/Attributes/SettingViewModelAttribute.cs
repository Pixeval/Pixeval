using System;

namespace Pixeval.Attributes;

public class SettingViewModelAttribute : Attribute
{
    public SettingViewModelAttribute(Type settingType, string settingName)
    {
        SettingType = settingType;
        SettingName = settingName;
    }
    public Type SettingType { get; }

    public string SettingName { get; }
}