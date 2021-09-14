using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Pixeval.Controls
{
    public class EnhancedPage : Page
    {
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Prepare(e);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            Dispose(e);
        }

        public virtual void Dispose(NavigatingCancelEventArgs e)
        {
        }

        public virtual void Prepare(NavigationEventArgs e)
        {
        }
    }
}