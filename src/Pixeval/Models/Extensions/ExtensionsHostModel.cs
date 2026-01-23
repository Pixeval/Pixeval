// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Pixeval.Extensions.Common;
using Pixeval.Utilities;
using Symbol = FluentIcons.Common.Symbol;
using SymbolIcon = FluentIcons.Avalonia.SymbolIcon;

namespace Pixeval.Models.Extensions;

public record ExtensionsHostModel(IExtensionsHost Host) : IDisposable
{
    public IExtensionsHost Host { get; } = Host;

    public bool IsActive
    {
        get => Values.TryGetTargetOrAddDefault(nameof(IsActive), true);
        set => Values.SetTarget(nameof(IsActive), value);
    }

    public int Priority
    {
        get => Values.TryGetTargetOrAddDefault(nameof(Priority), 0);
        set => Values.SetTarget(nameof(Priority), value);
    }

    internal nint Handle { get; init; }

    public string Name { get; } = Host.ExtensionName;

    public string Description { get; } = Host.Description;

    public string Author { get; } = Host.AuthorName;

    public string Version { get; } = Host.Version;

    public Uri? Link { get; } = Uri.TryCreate(Host.ExtensionLink, UriKind.RelativeOrAbsolute, out var uri) ? uri : null;

    public Uri? HelpLink { get; } = Uri.TryCreate(Host.HelpLink, UriKind.RelativeOrAbsolute, out var uri) ? uri : null;

    private Bitmap? IconImageSource { get; } = GetIconSource(Host.Icon);

    public Control Icon => IconImageSource is null
        ? new SymbolIcon { Symbol = Symbol.PuzzlePiece }
        : new Image { Source = IconImageSource };

    public Dictionary<string, JsonElement> Values { get; } = GetValues(Host);

    public IReadOnlyList<IExtension> Extensions { get; } = Host.Extensions;

    private static Dictionary<string, JsonElement> GetValues(IExtensionsHost host)
    {
        ref var value = ref CollectionsMarshal.GetValueRefOrAddDefault(App.AppViewModel.AppSettings.ExtensionSettings, host.ExtensionName, out var exists);
        if (!exists)
            value = [];
        return value!;
    }

    private static Bitmap? GetIconSource(byte[]? iconBytes)
    {
        return iconBytes is null ? null : new Bitmap(Streams.RentStream(iconBytes));
    }

    private static Control GetIcon(Bitmap? imageSource)
    {
        return imageSource is null
            ? new SymbolIcon { Symbol = Symbol.PuzzlePiece }
            : new Image { Source = imageSource };
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        NativeLibrary.Free(Handle);
    }
}
