using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Pixeval.Util
{
    public class EnhancedPage : Page, IDisposable
    {
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Prepare();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            Dispose();
        }

        public virtual void Dispose()
        {
        }

        public virtual void Prepare()
        {
        }
    }
}