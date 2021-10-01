using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Pixeval.Controls
{
    public class IconButton : Button
    {
        public IconButton()
        {
            Content = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Children =
                {
                    new ContentPresenter
                    {
                        VerticalAlignment = VerticalAlignment.Center
                    },
                    new TextBlock
                    {
                        Margin = new Thickness(5, 0, 0, 0),
                        VerticalAlignment = VerticalAlignment.Center
                    }
                }
            };
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(IconButton),
            PropertyMetadata.Create(string.Empty, TextPropertyChangedCallback));

        public string Text
        {
            get => (string) GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        private static void TextPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textBlock = ((StackPanel) ((Button) d).Content).FindDescendant<TextBlock>();
            if (e.NewValue is null)
            {
                textBlock!.Visibility = Visibility.Collapsed;
                return;
            }
            textBlock!.Text = (string) e.NewValue;
        }

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
            nameof(Icon),
            typeof(IconElement),
            typeof(IconButton),
            PropertyMetadata.Create(DependencyProperty.UnsetValue, IconChangedCallback));

        public IconElement Icon
        {
            get => (IconElement) GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        private static void IconChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((StackPanel) ((Button) d).Content).FindDescendant<ContentPresenter>()!.Content = e.NewValue;
        }
    }
}