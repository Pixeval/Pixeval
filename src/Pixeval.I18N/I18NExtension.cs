using System;
using Avalonia;
using Avalonia.Metadata;

namespace Pixeval.I18N;

public class I18NExtension : AvaloniaObject
{
    public I18NExtension()
    {
    }

    public I18NExtension(string key)
    {
        Key = key;
    }

    [Content]
    public string Key { get; set; } = "";

    public string ProvideValue(IServiceProvider serviceProvider)
    {
        return I18NManager.GetResource(Key ?? "");
    }
}
