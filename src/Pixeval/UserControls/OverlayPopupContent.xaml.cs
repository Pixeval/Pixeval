using Microsoft.UI.Xaml;

namespace Pixeval.UserControls
{
    public sealed partial class OverlayPopupContent
    {
        public static DependencyProperty PopupContentProperty = DependencyProperty.Register(
            nameof(PopupContent),
            typeof(object),
            typeof(OverlayPopupContent),
            PropertyMetadata.Create(DependencyProperty.UnsetValue, (o, args) =>
            {
                var popupContent = (OverlayPopupContent) o;
                var element = (FrameworkElement) args.NewValue;
                popupContent.PopupContentPresenter.Content = element;
                element.Shadow = popupContent.PopupContentShadow;
            }));

        public object PopupContent
        {
            get => GetValue(PopupContentProperty);
            set => SetValue(PopupContentProperty, value);
        }

        public OverlayPopupContent()
        {
            InitializeComponent();
        }

        private void OverlayPopupContent_OnLoaded(object sender, RoutedEventArgs e)
        {
            PopupContentShadow.Receivers.Add(ShadowReceiver);
        }
    }
}
