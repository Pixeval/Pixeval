using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Pixeval
{
    class Program
    {
        [STAThread]
        public static void Main()
        {
            var app = new App();
            if (app.MainWindow.Dispatcher != null) app.MainWindow.Dispatcher.UnhandledException += Dispatcher_UnhandledException;
            app.Run();
        }

        private static void Dispatcher_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
#if RELEASE
            switch (e.Exception)
            {
                case QueryNotRespondingException _:
                    MessageQueue.Enqueue(AkaI18N.QueryNotResponding);
                    break;
                case ApiException apiException:
                    if (apiException.StatusCode == HttpStatusCode.BadRequest)
                        MessageQueue.Enqueue(AkaI18N.QueryNotResponding);
                    break;
                case HttpRequestException _: break;
                default:
                    ExceptionDumper.WriteException(e.Exception);
                    break;
            }

            e.Handled = true;
#endif
        }
    }
}
