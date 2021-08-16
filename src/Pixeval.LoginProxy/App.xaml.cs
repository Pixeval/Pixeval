using System.Globalization;
using System.Linq;
using System.Windows;

namespace Pixeval.LoginProxy
{
    public partial class App
    {
        public static string? Culture;

        protected override void OnStartup(StartupEventArgs e)
        {
            Culture = e.Args.FirstOrDefault() ?? CultureInfo.CurrentUICulture.ToString();
        }
    }
}
