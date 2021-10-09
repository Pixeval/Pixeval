using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using Pixeval.CommunityToolkit;
using Pixeval.Misc;

namespace Pixeval.Util.UI
{
    [MarkupExtensionReturnType(ReturnType = typeof(FontIcon))]
    public class FontIconExtension : TextIconExtension
    {
        public FontIconSymbols Glyph { get; set; }

        public FontFamily? FontFamily { get; set; }

        /// <inheritdoc/>
        protected override object ProvideValue()
        {
            var fontIcon = new FontIcon
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