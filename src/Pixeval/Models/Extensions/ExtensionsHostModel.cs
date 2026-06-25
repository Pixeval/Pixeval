// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using FluentIcons.Avalonia;
using FluentIcons.Common;
using Pixeval.Extensions.Common;
using Pixeval.Utilities;

namespace Pixeval.Models.Extensions;

public class ExtensionsHostModel(
    IExtensionsHost host,
    Dictionary<string, object?> values) : IDisposable
{
    public IExtensionsHost Host { get; } = host;

    public bool IsActive
    {
        get => Values.TryGetTargetOrAddDefault(nameof(IsActive), true);
        set => Values[nameof(IsActive)] = value;
    }

    public int Priority
    {
        get => Values.TryGetTargetOrAddDefault(nameof(Priority), 0);
        set => Values[nameof(Priority)] = value;
    }

    internal nint Handle { get; init; }

    public string Name { get; } = host.ExtensionName;

    public string Description { get; } = host.Description;

    public string Author { get; } = host.AuthorName;

    public string Version { get; } = host.Version;

    public Uri? Link { get; } = Uri.TryCreate(host.ExtensionLink, UriKind.RelativeOrAbsolute, out var uri) ? uri : null;

    public Uri? HelpLink { get; } = Uri.TryCreate(host.HelpLink, UriKind.RelativeOrAbsolute, out var uri) ? uri : null;

    /// <summary>
    /// 不能缓存<see cref="ExtensionsHostModel.Icon"/>
    /// </summary>
    public Control Icon
    {
        get
        {
            try
            {
                if (Host.Icon is { Length: > 0 } icon)
                    return new Image { Source = new Bitmap(Streams.RentStream(icon)) };
            }
            catch
            {
                // ignored
            }

            return CreateFallbackIcon();
        }
    }

    public Dictionary<string, object?> Values { get; } = values;

    public IReadOnlyList<IExtension> Extensions { get; } = host.Extensions;

    private static SymbolIcon CreateFallbackIcon() => new() { Symbol = Symbol.PuzzlePiece };

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        NativeLibrary.Free(Handle);
    }
}
