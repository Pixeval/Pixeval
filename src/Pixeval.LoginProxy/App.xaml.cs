using System;
using System.Windows;

namespace Pixeval.LoginProxy
{
    public partial class App
    {
        public static string? Culture;

        // reserved: public static bool? SignUp;

        protected override void OnStartup(StartupEventArgs e)
        {
#if DEBUG
            Culture = "zh-CN";
#else
            Environment.Exit(-1);
#endif
        }
    }
}
