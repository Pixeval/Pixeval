using Microsoft.UI.Xaml;

namespace Pixeval.Controls.Setting.UI.UserControls
{
    public sealed partial class SettingEntryHeader
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

        private static void HeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is string str)
            {
                ((SettingEntryHeader) d).HeaderTextBlock.Text = str;
            }
        }

        private static void DescriptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is { } content)
            {
                ((SettingEntryHeader) d).DescriptionPresenter.Content = content;
            }
        }

        public SettingEntryHeader()
        {
            InitializeComponent();
        }
    }
}
