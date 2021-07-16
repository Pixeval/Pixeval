using System;
using System.Windows;

namespace Pixeval.LoginProxy
{
    public partial class App
    {
        public static int Port;

        public static string? Culture;

        // reserved: public static bool? SignUp;

        protected override void OnStartup(StartupEventArgs e)
        {
#if DEBUG
            Culture = "zh-CN";
            Port = 6133;
#else
            if (e.Args.Length >= 2 && int.TryParse(e.Args[0], out _))
            {
                Culture = e.Args[1];
                Port = int.Parse(e.Args[0]);
                // reserved: SignUp = e.Args.Length >= 3 && e.Args[2] == "-signUp";
                AppDomain.CurrentDomain.UnhandledException += (_, args) =>
                {
                    if (args.ExceptionObject is PixivWebLoginException webLoginException)
                    {
                        Interop.PostJsonToPixevalClient("/login/token", new LoginTokenRequest
                        {
                            Errno = (int) webLoginException.Reason
                        }).ContinueWith(t =>
                        {
                            if (t.Exception is not null || !t.Result.IsSuccessStatusCode)
                            {
                                MessageBox.Show("Error when communicating to Pixeval client", "Unexpected Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }).ContinueWith(_ => Environment.Exit(-1));
                    }
                };
                return;

            }

            MessageBox.Show("Illegal Arguments", "Unexpected Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Environment.Exit(-1);
#endif
        }
    }
}
