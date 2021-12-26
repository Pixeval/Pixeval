using Microsoft.UI.Xaml;
using Pixeval.Attributes;

namespace Pixeval.Controls.SlideView
{
    [DependencyProperty("ContentTemplate", typeof(DataTemplate), nameof(OnContentTemplateChanged))]
    [DependencyProperty("SlideContent", typeof(object), nameof(OnSlideContentChanged))]
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
