using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Pixeval.ViewModel;

namespace Pixeval.Pages.IllustrationViewer
{
    public sealed partial class IllustrationInfoPage
    {
        private IllustrationViewerPageViewModel _viewModel = null!;

        public IllustrationInfoPage()
        {
            InitializeComponent();
        }

        public override void Prepare(NavigationEventArgs e)
        {
            _viewModel = (IllustrationViewerPageViewModel) e.Parameter;
        }
    }
}
