using Microsoft.UI.Xaml;
using Pixeval.Misc;

namespace Pixeval.Controls.SlideView
{
    [DependencyProperty("ContentTemplate", typeof(DataTemplate), InstanceChangedCallback = true)]
    [DependencyProperty("SlideContent", typeof(object), InstanceChangedCallback = true)]
    public sealed partial class Slide
    {
        public Slide()
        {
            InitializeComponent();
        }

        private static void OnContentTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Slide slide && e.NewValue is DataTemplate template)
            {
                slide.ContentPresenter.ContentTemplate = template;
            }
        }

        private static void OnSlideContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Slide slide && e.NewValue is { } content)
            {
                slide.ContentPresenter.Content = content;
            }
        }
    }
}
