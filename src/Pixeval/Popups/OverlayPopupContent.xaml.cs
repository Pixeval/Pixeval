using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Pixeval.Util.UI;

namespace Pixeval.Popups
{
    public sealed partial class OverlayPopupContent
    {
        private readonly Guid _popupGuid;

        public static DependencyProperty PopupContentProperty = DependencyProperty.Register(
            nameof(PopupContent),
            typeof(object),
            typeof(OverlayPopupContent),
            PropertyMetadata.Create(DependencyProperty.UnsetValue, (o, args) =>
            {
                var popupContent = (OverlayPopupContent) o;
                var element = (FrameworkElement) args.NewValue;
                popupContent.PopupContentPresenter.Children.Add(element);
                element.Shadow = popupContent.PopupContentShadow;
            }));

        public object PopupContent
        {
            get => GetValue(PopupContentProperty);
            set => SetValue(PopupContentProperty, value);
        }

        public static DependencyProperty IsLightDismissProperty = DependencyProperty.Register(
            nameof(PopupContent),
            typeof(bool),
            typeof(OverlayPopupContent),
            PropertyMetadata.Create(false, (o, args) =>
            {
                var popupContent = (OverlayPopupContent) o;
                var value = (bool) args.NewValue;
                if (value)
                {
                    popupContent.ShadowReceiver.Tapped += OnShadowReceiverOnTapped;
                }
                else
                {
                    popupContent.ShadowReceiver.Tapped -= OnShadowReceiverOnTapped;
                }

                void OnShadowReceiverOnTapped(object sender, TappedRoutedEventArgs a)
                {
                    PopupManager.ClosePopup(PopupManager.OpenPopups[popupContent._popupGuid]);
                }
            }));

        public bool IsLightDismiss
        {
            get => (bool) GetValue(IsLightDismissProperty);
            set => SetValue(IsLightDismissProperty, value);
        }

        public OverlayPopupContent(Guid popupGuid)
        {
            _popupGuid = popupGuid;
            InitializeComponent();
        }

        private void OverlayPopupContent_OnLoaded(object sender, RoutedEventArgs e)
        {
            PopupContentShadow.Receivers.Add(ShadowReceiver);
        }
    }
}
