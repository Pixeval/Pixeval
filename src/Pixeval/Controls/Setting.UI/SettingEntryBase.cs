using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.Controls.Setting.UI.UserControls;

namespace Pixeval.Controls.Setting.UI
{
    public class SettingEntryBase : Control
    {
        protected const string PartEntryHeader = "EntryHeader";

        protected SettingEntryHeader? SettingEntryHeader;

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
            nameof(Icon),
            typeof(IconElement),
            typeof(SettingEntryBase),
            PropertyMetadata.Create(DependencyProperty.UnsetValue, (o, args) => IconChanged(o, args.NewValue)));

        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
            nameof(Header),
            typeof(string),
            typeof(SettingEntryBase),
            PropertyMetadata.Create(DependencyProperty.UnsetValue));

        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
            nameof(Description),
            typeof(object),
            typeof(SettingEntryBase),
            PropertyMetadata.Create(DependencyProperty.UnsetValue, (o, args) => DescriptionChanged(o, args.NewValue)));

        public IconElement Icon
        {
            get => (IconElement) GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public string Header
        {
            get => (string) GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public object Description
        {
            get => GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        private static void IconChanged(DependencyObject dependencyObject, object argsNewValue)
        {
            if (dependencyObject is SettingEntryBase {SettingEntryHeader: { } header})
            {
                header.Margin = argsNewValue is IconElement
                    ? new Thickness(0)
                    : new Thickness(10, 0, 0, 0);
            }
        }

        private static void DescriptionChanged(DependencyObject dependencyObject, object? argsNewValue)
        {
            if (dependencyObject is SettingEntryBase {SettingEntryHeader: { } header})
            {
                if (argsNewValue is UIElement element)
                {
                    header.Description = element;
                    return;
                }

                header.Description = new TextBlock
                {
                    Text = argsNewValue?.ToString() ?? string.Empty
                };
            }
        }

        public SettingEntryBase()
        {
            Loaded += (_, _) =>
            {
                IconChanged(this, Icon);
                DescriptionChanged(this, Description);
                Update();
            };
        }

        protected virtual void Update()
        {
            
        }

        protected override void OnApplyTemplate()
        {
            SettingEntryHeader = (SettingEntryHeader?) GetTemplateChild(PartEntryHeader);
            base.OnApplyTemplate();
        }
    }
}