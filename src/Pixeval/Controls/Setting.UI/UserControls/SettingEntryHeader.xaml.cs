using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

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

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
            nameof(Icon),
            typeof(IconElement),
            typeof(SettingEntryHeader),
            PropertyMetadata.Create(DependencyProperty.UnsetValue, IconChanged));

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

        public IconElement Icon
        {
            get => (IconElement) GetValue(IconProperty);
            set => SetValue(IconProperty, value);
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

        private static void IconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var presenter = ((SettingEntryHeader)d).IconContentPresenter;
            if (e.NewValue is IconElement icon)
            {
                presenter.Visibility = Visibility.Visible;
                presenter.Content = icon;
            }
            else
            {
                presenter.Visibility = Visibility.Collapsed;
            }
        }

        public SettingEntryHeader()
        {
            InitializeComponent();
            Loaded += (_, _) =>
            {
                // Prevent the button from expanding the expander
                if (DescriptionPresenter.Content is DependencyObject obj && obj.FindDescendantOrSelf<ButtonBase>() is { } buttonBase)
                {
                    buttonBase.Tapped += (_, eventArgs) => eventArgs.Handled = true;
                }
            };
        }
    }
}
