using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Pixeval.Popups.IllustrationResultFilter
{
    public class IllustrationResultFilterFunctionEntry : ContentControl
    {
        public IllustrationResultFilterFunctionEntry()
        {
            DefaultStyleKey = typeof(IllustrationResultFilterFunctionEntry);
        }

        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
            nameof(Header), 
            typeof(string), 
            typeof(IllustrationResultFilterFunctionEntry),
            new PropertyMetadata(DependencyProperty.UnsetValue));

        public string Header
        {
            get => (string) GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }
    }
}