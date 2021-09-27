using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace Pixeval.Popups
{
    public sealed partial class QrCodePresenter
    {
        private readonly ImageSource _qrCodeImageSource;

        public QrCodePresenter(ImageSource qrCodeImageSource)
        {
            _qrCodeImageSource = qrCodeImageSource;
            InitializeComponent();
        }

        private void QrCodeImage_OnLoaded(object sender, RoutedEventArgs e)
        {
            QrCodeImage.Source = _qrCodeImageSource;
        }
    }
}
