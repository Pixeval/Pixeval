using System;
using Windows.System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Symbol = FluentIcons.Common.Symbol;

namespace Pixeval.Settings;

public abstract class SettingsEntryBase<TSettings>(
    TSettings settings,
    string header,
    string description,
    Symbol headerIcon) : ISettingsEntry
{
    public abstract FrameworkElement Element { get; }

    public Symbol HeaderIcon { get; set; } = headerIcon;

    public string Header { get; set; } = header;

    public object DescriptionControl
    {
        get
        {
            if (DescriptionUri is not null)
            {
                var b = new HyperlinkButton { Content = Description };
                if (DescriptionUri.Scheme is "http" or "https")
                {
                    b.NavigateUri = DescriptionUri;
                    return b;
                }

                var uri = DescriptionUri;
                b.Click += (_, _) => _ = Launcher.LaunchUriAsync(uri);
                return b;
            }
            return Description;
        }
    }

    public string Description { get; set; } = description;

    public Uri? DescriptionUri { get; set; }

    public TSettings Settings { get; } = settings;

    public abstract void ValueReset();

    public virtual void ValueSaving()
    {
    }
}
