// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Pixeval.Themes;

internal partial class MicaStyles : ResourceDictionary
{
    public MicaStyles()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
