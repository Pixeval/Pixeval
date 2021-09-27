using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Pixeval.Controls
{
    public class EnhancedPage : Page
    {
        public EnhancedPage()
        {
            Loaded += (_, _) =>
            {
                if (!Initialized)
                {
                    Initialized = true;
                }
            };
        }

        public bool Initialized { get; private set; }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            OnPageActivated(e);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            OnPageDeactivated(e);
        }

        public virtual void OnPageDeactivated(NavigatingCancelEventArgs e)
        {
        }

        public virtual void OnPageActivated(NavigationEventArgs e)
        {
        }
    }
}