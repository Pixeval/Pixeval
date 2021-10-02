using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Markup;

namespace Pixeval.Controls.Setting.UI.ExpanderSettingEntry
{
    [ContentProperty(Name = nameof(Content))]
    public class ExpanderSettingEntry : SettingEntryBase
    {
        public static readonly DependencyProperty HeaderHeightProperty = DependencyProperty.Register(
            nameof(HeaderHeight),
            typeof(double),
            typeof(ExpanderSettingEntry),
            PropertyMetadata.Create(DependencyProperty.UnsetValue));

        public double HeaderHeight
        {
            get => (double)GetValue(HeaderHeightProperty);
            set => SetValue(HeaderHeightProperty, value);
        }


        public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
            nameof(Content), 
            typeof(object), 
            typeof(ExpanderSettingEntry),
            PropertyMetadata.Create(DependencyProperty.UnsetValue));

        public object Content
        {
            get => GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        public ExpanderSettingEntry()
        {
            DefaultStyleKey = typeof(ExpanderSettingEntry);
        }
    }
}