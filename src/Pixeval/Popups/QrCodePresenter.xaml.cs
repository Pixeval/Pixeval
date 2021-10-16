using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace Pixeval.Popups
{
    public sealed partial class QrCodePresenter : IAppPopupContent
    {
        private readonly ImageSource _qrCodeImageSource;

        public Guid UniqueId { get; }

        public FrameworkElement UIContent => this;

        public QrCodePresenter(ImageSource qrCodeImageSource)
        {
            _qrCodeImageSource = qrCodeImageSource;
            UniqueId = Guid.NewGuid();
            InitializeComponent();
        }

        private void QrCodeImage_OnLoaded(object sender, RoutedEventArgs e)
        {
            QrCodeImage.Source = _qrCodeImageSource;
        }
    }
}
