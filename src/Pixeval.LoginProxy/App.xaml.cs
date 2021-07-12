using System;
using System.Linq;
using System.Windows;

namespace Pixeval.LoginProxy
{
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            if (e.Args.Length < 2 || !int.TryParse(e.Args[0], out _))
            {
                var mainWindow = new MainWindow(e.Args[1], int.Parse(e.Args[0]));
                mainWindow.Show();
                return;
            }

            Environment.Exit(-1);
        }
    }
}
