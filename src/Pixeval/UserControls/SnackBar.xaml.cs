#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/SnackBar.xaml.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

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
            get => (UIElement)GetValue(ShadowReceiverProperty);
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
