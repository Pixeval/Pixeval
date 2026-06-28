// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using Avalonia.Media;
using Avalonia.Metadata;

namespace Pixeval.Views.Markup;

public class SvgPathExtension(string path)
{
    [ConstructorArgument("path")]
    public string Path { get; set; } = path;

    public Geometry? ProvideValue(IServiceProvider serviceProvider) => string.IsNullOrWhiteSpace(Path) ? null : Geometry.Parse(Path);
}
