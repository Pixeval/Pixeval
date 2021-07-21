using System;
using System.Threading.Tasks;
using Mako;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.Events;
using Pixeval.Util;

namespace Pixeval
{
    public partial class App
    {
        public static MainWindow Window = null!;

        public static MakoClient MakoClient { get; set; } = null!; // The null-state of MakoClient is transient

        public App()
        {
            InitializeComponent();

            UnhandledException += async (_, args) =>
            {
                args.Handled = true;
                await MessageDialogBuilder.Create()
                    .WithTitle(MiscResources.ExceptionEncountered)
                    .WithContent(args.Message)
                    .WithPrimaryButtonText(MessageContentDialogResources.OkButtonContent)
                    .WithDefaultButton(ContentDialogButton.Primary)
                    .Build(Window)
                    .ShowAsync();
                await ExitWithPushedNotification();
            };
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        { 
            Window = new MainWindow();
            Window.SetWindowSize(800, 600);
            Window.Activate();
        }

        /// <summary>
        /// Exit the notification after pushing an <see cref="ApplicationExitingEvent"/>
        /// to the <see cref="EventChannel"/>
        /// </summary>
        /// <returns></returns>
        public static async Task ExitWithPushedNotification()
        {
            await EventChannel.Default.PublishAsync(new ApplicationExitingEvent());
            await Task.Delay(200); // well...just wait a second to let those subscribers handle the event
        }
    }
}
