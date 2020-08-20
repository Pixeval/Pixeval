using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Pixeval.Wpf.View;

namespace Pixeval.Wpf
{
    class Worker:BackgroundService
    {
        private readonly App _app;
        private readonly MainWindow _mainWindow;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;

        public Worker(App app, MainWindow mainWindow)
        {
            _app = app;
            _mainWindow = mainWindow;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _app.Run(_mainWindow);
            
            return Task.CompletedTask;
        }
    }
}
