using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Pixeval.UI;

namespace Pixeval
{
    class Worker:BackgroundService
    {
        private readonly App _app;
        private readonly MainWindow _mainWindow;

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
