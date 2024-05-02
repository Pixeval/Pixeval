using System.Reflection;
using Pixeval.Attributes;
using WinUI3Utilities;

namespace Pixeval.Settings;

public abstract class SingleValueSettingsEntryBase<TSettings> : ObservableSettingsEntryBase<TSettings>
{
    protected SingleValueSettingsEntryBase(TSettings settings,
        string propertyName) : base(settings, "", "", default)
    {
        PropertyInfo = typeof(TSettings).GetProperty(propertyName) ?? ThrowHelper.Argument<string, PropertyInfo>(propertyName);
        Attribute = PropertyInfo.GetCustomAttribute<SettingsEntryAttribute>();
        if (Attribute is { } attribute)
        {
            Header = attribute.LocalizedResourceHeader;
            Description = attribute.LocalizedResourceDescription;
            HeaderIcon = attribute.IconGlyph;
        }
    }

    public PropertyInfo PropertyInfo { get; }

    public SettingsEntryAttribute? Attribute { get; }

    public object? ValueBase
    {
        get => PropertyInfo.GetValue(Settings);
        set => PropertyInfo.SetValue(Settings, value);
    }

    public override void ValueReset() => OnPropertyChanged("Value");
}
