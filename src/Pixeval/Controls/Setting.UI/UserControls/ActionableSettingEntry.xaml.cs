using Microsoft.UI.Xaml;

namespace Pixeval.Controls.Setting.UI.UserControls
{
    public sealed partial class ActionableSettingEntry
    {
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
            nameof(Header),
            typeof(string),
            typeof(SettingEntryHeader),
            PropertyMetadata.Create(DependencyProperty.UnsetValue, HeaderChanged));

        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
            nameof(Description),
            typeof(object),
            typeof(SettingEntryHeader),
            PropertyMetadata.Create(DependencyProperty.UnsetValue, DescriptionChanged));

        public static readonly DependencyProperty ActionContentProperty = DependencyProperty.Register(
            nameof(ActionContent),
            typeof(object),
            typeof(SettingEntryHeader),
            PropertyMetadata.Create(DependencyProperty.UnsetValue, ActionContentChanged));

        public string Header
        {
            get => (string)GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }
        public object Description
        {
            get => GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        public object ActionContent
        {
            get => GetValue(ActionContentProperty);
            set => SetValue(ActionContentProperty, value);
        }

        private static void HeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is string str)
            {
                ((ActionableSettingEntry) d).EntryHeader.Header = str;
            }
        }

        private static void DescriptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is { } content)
            {
                ((ActionableSettingEntry) d).EntryHeader.Description = content;
            }
        }

        private static void ActionContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is { } content)
            {
                ((ActionableSettingEntry) d).ActionContainer.Content = content;
            }
        }


        public ActionableSettingEntry()
        {
            InitializeComponent();
        }
    }
}
