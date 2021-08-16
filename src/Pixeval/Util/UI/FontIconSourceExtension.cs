using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;

namespace Pixeval.Util.UI
{
    [MarkupExtensionReturnType(ReturnType = typeof(FontIconSource))]
    public class FontIconSourceExtension : TextIconExtension
    {
        public FontIconSymbols Glyph { get; set; }

        public FontFamily? FontFamily { get; set; }

        /// <inheritdoc/>
        protected override object ProvideValue()
        {
            var fontIcon = new FontIconSource
            {
                Glyph = Glyph.GetMetadataOnEnumMember(),
                FontFamily = FontFamily ?? SegoeMDL2AssetsFontFamily,
                FontWeight = FontWeight,
                FontStyle = FontStyle,
                IsTextScaleFactorEnabled = IsTextScaleFactorEnabled,
                MirroredWhenRightToLeft = MirroredWhenRightToLeft
            };

            if (FontSize > 0)
            {
                fontIcon.FontSize = FontSize;
            }

            if (Foreground != null)
            {
                fontIcon.Foreground = Foreground;
            }

            return fontIcon;
        }
    }
}