using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Markup;

namespace Pixeval.Controls.Setting.UI.ActionableSettingEntry
{
    [ContentProperty(Name = nameof(ActionContent))]
    public class ActionableSettingEntry : SettingEntryBase
    {
        public static readonly DependencyProperty ActionContentProperty = DependencyProperty.Register(
            nameof(ActionContent),
            typeof(object),
            typeof(ActionableSettingEntry),
            PropertyMetadata.Create(DependencyProperty.UnsetValue));

        public object ActionContent
        {
            get => GetValue(ActionContentProperty);
            set => SetValue(ActionContentProperty, value);
        }

        public ActionableSettingEntry()
        {
            DefaultStyleKey = typeof(ActionableSettingEntry);
        }
    }
}