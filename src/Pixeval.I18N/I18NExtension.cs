using System;
using Avalonia;
using Avalonia.Metadata;

namespace Pixeval.I18N;

public class I18NExtension(string key) : AvaloniaObject
{
    public static readonly DirectProperty<I18NExtension, string> KeyProperty = AvaloniaProperty.RegisterDirect<I18NExtension, string>(
        nameof(Key),
        o => o.Key,
        (t, v) => t.Key = v);

    [Content]
    public string Key
    {
        get;
        set => SetAndRaise(KeyProperty, ref field, value);
    } = key;

    public string ProvideValue(IServiceProvider serviceProvider)
    {
        return I18NManager.GetResource(Key ?? "");
    }
}
