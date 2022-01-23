using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Pixeval.Util.Threading;

namespace Pixeval.UserControls
{
    public sealed partial class SnackBar
    {
        public static readonly DependencyProperty ShadowReceiverProperty = DependencyProperty.Register(
            nameof(ShadowReceiver),
            typeof(UIElement),
            typeof(SnackBar),
            PropertyMetadata.Create(DependencyProperty.UnsetValue, ShadowReceiverChanged));

        public UIElement ShadowReceiver
        {
            get => (UIElement) GetValue(ShadowReceiverProperty);
            set => SetValue(ShadowReceiverProperty, value);
        }

        private static void ShadowReceiverChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is UIElement element && d is SnackBar { SnackShadow: var shadow })
            {
                shadow.Receivers.Add(element);
            }
        }

        private CancellationTokenSource? _hideSnackBarTokenSource;

        public SnackBar()
        {
            InitializeComponent();
        }

        public async void Show(string text, int duration)
        {
            _hideSnackBarTokenSource?.Cancel();
            _hideSnackBarTokenSource = new CancellationTokenSource();
            SnackBarContent.Text = text;
            SnackBarContentContainer.Visibility = Visibility.Visible;
            SnackBarContentContainer.Opacity = 1;
            await Task.Delay(200);
            Task.Delay(duration, _hideSnackBarTokenSource.Token).ContinueWith(t =>
            {
                if (!t.IsCanceled)
                {
                    App.AppViewModel.DispatchTask(async () =>
                    {
                        SnackBarContentContainer.Opacity = 0;
                        await Task.Delay(200);
                        SnackBarContentContainer.Visibility = Visibility.Collapsed;
                    });
                }
            }).Discard();
        }
    }
}
