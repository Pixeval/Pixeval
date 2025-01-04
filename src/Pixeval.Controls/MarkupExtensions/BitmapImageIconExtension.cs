// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Pixeval.Controls;

[MarkupExtensionReturnType(ReturnType = typeof(ImageIcon))]
public partial class BitmapImageIconExtension : MarkupExtension
{
    public string Uri { get; set; } = "";

    protected override object ProvideValue()
    {
        return new ImageIcon { Source = new BitmapImage(new(Uri)) };
    }
}
