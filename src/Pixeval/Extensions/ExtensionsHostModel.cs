// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.IO;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.Win32;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.Windows.Storage;
using Pixeval.Extensions.Common;
using Symbol = FluentIcons.Common.Symbol;
using SymbolIcon = FluentIcons.WinUI.SymbolIcon;

namespace Pixeval.Extensions;

public partial record ExtensionsHostModel(IExtensionsHost Host) : IDisposable
{
    public bool IsActive
    {
        get
        {
            if (Values.TryGetValue(nameof(IsActive), out var value) && value is bool v)
                return v;

            Values[nameof(IsActive)] = true;
            return true;
        }
        set => Values[nameof(IsActive)] = value;
    }

    internal FreeLibrarySafeHandle? Handle { get; init; }

    public string Name { get; } = Host.GetExtensionName();

    public string Description { get; } = Host.GetDescription();

    public string Author { get; } = Host.GetAuthorName();

    public string Version { get; } = Host.GetVersion();

    public Uri Link { get; } = new Uri(Host.GetExtensionLink());

    public Uri? HelpLink { get; } = Host.GetHelpLink() is { } helpLink ? new Uri(helpLink) : null;

    private BitmapImage? IconImageSource { get; } = GetIconSource(Host.GetIcon());

    public IconElement Icon => IconImageSource is null
        ? new SymbolIcon { Symbol = Symbol.PuzzlePiece }
        : new ImageIcon { Source = IconImageSource };

    public IPropertySet Values { get; } = GetValues(Host);

    public IReadOnlyList<IExtension> Extensions { get; } = Host.GetExtensions();

    private static IPropertySet GetValues(IExtensionsHost host)
    {
        var localSettings = ApplicationData.GetDefault().LocalSettings;
        var extensionName = host.GetExtensionName();
        if (!localSettings.Containers.TryGetValue(extensionName, out var container))
            container = localSettings.CreateContainer(extensionName, ApplicationDataCreateDisposition.Always);
        return container.Values;
    }

    private static BitmapImage? GetIconSource(byte[]? iconBytes)
    {
        if (iconBytes is null)
            return null;
        var image = new BitmapImage();
        using var stream = new InMemoryRandomAccessStream();
        var s = stream.AsStreamForWrite();
        s.Write(iconBytes);
        s.Flush();
        stream.Seek(0);
        image.SetSource(stream);
        return image;
    }

    private static IconElement GetIcon(BitmapImage? imageSource)
    {
            return imageSource is null ?  new SymbolIcon { Symbol = Symbol.PuzzlePiece } : new ImageIcon { Source = imageSource };
    }

    public void Dispose()
    {
        Handle?.Dispose();
        GC.SuppressFinalize(this);
    }
}
