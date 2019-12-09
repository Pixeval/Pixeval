using System.Windows;

namespace Pixeval.Objects
{
    public static class Controls
    {
        public static void Unable(this FrameworkElement element)
        {
            element.IsEnabled = false;
        }

        public static void Enable(this FrameworkElement element)
        {
            element.IsEnabled = true;
        }
    }
}