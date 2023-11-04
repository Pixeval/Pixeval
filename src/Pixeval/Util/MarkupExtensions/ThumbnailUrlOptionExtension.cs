using Microsoft.UI.Xaml.Markup;
using Pixeval.Options;

namespace Pixeval.Controls.MarkupExtensions;

[MarkupExtensionReturnType(ReturnType = typeof(ThumbnailUrlOption))]
public class ThumbnailUrlOptionExtension : MarkupExtension
{
    public ThumbnailUrlOption Enum { get; set; }

    /// <inheritdoc />
    protected override object ProvideValue() => Enum;
}
