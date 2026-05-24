// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Avalonia.Media;
using FluentIcons.Common;

namespace Pixeval.Views.Home;

public sealed record HomeCardPreviewItem(
    string Title,
    string Subtitle,
    string? ImageUrl,
    Symbol Symbol);

public sealed record HomeCardPreviewCell(HomeCardPreviewItem Item, IBrush FallbackBrush);
