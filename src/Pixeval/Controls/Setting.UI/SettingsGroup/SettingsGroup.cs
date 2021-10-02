using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Pixeval.Controls.Setting.UI.SettingsGroup
{
    public class SettingsGroup : ItemsControl
    {
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
            nameof(Header), 
            typeof(string), 
            typeof(SettingsGroup), 
            PropertyMetadata.Create(DependencyProperty.UnsetValue));

        public string Header
        {
            get => (string) GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public SettingsGroup()
        {
            DefaultStyleKey = typeof(SettingsGroup);
        }
    }
}