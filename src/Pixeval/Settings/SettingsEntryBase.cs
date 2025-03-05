// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation.Collections;
using Windows.System;
using WinUI3Utilities;
using Symbol = FluentIcons.Common.Symbol;

namespace Pixeval.Settings;

public abstract class SettingsEntryBase(
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

    public virtual Uri? DescriptionUri { get; set; }

    public abstract void ValueSaving(IPropertySet values);

    public static string SubHeader(WorkTypeEnum workType) => workType switch
    {
        WorkTypeEnum.Illustration => SettingsPageResources.IllustrationOptionEntryHeader,
        WorkTypeEnum.Manga => SettingsPageResources.MangaOptionEntryHeader,
        WorkTypeEnum.Ugoira => SettingsPageResources.UgoiraOptionEntryHeader,
        WorkTypeEnum.Novel => SettingsPageResources.NovelOptionEntryHeader,
        _ => ThrowHelper.ArgumentOutOfRange<WorkTypeEnum, string>(workType)
    };

    public static Symbol SubHeaderIcon(WorkTypeEnum workType) => workType switch
    {
        WorkTypeEnum.Illustration => Symbol.Image,
        WorkTypeEnum.Manga => Symbol.ImageMultiple,
        WorkTypeEnum.Ugoira => Symbol.Gif,
        WorkTypeEnum.Novel => Symbol.BookOpen,
        _ => ThrowHelper.ArgumentOutOfRange<WorkTypeEnum, Symbol>(workType)
    };
}
