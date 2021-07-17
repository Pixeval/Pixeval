using System;
using Mako;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.Events;
using Pixeval.Interop;
using Pixeval.Util;
using WinRT;

namespace Pixeval
{
    public partial class App
    {
        public static MainWindow Window = null!;

        public static readonly EventChannel PixevalEventChannel = EventChannel.CreateStarted();

        public static MakoClient? PixevalAppClient { get; set; }

        public App()
        {
            InitializeComponent();

            UnhandledException += async (_, args) =>
            {
                args.Handled = true;
                await PixevalEventChannel.PublishAsync(new ApplicationShutdownAbnormallyEvent());
                await MessageDialogBuilder.Create()
                    .WithTitle(MiscResources.ExceptionEncountered)
                    .WithContent(args.Message)
                    .WithPrimaryButtonText(MessageContentDialogResources.OkButtonContent)
                    .WithDefaultButton(ContentDialogButton.Primary)
                    .Build(Window)
                    .ShowAsync();
                Current.Exit();
            };
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        { 
            Window = new MainWindow();
            var windowHandle = Window.As<IWindowNative>(); // see https://github.com/microsoft/WinUI-3-Demos/blob/master/src/Build2020Demo/DemoBuildCs/DemoBuildCs/DemoBuildCs/App.xaml.cs
            UIHelper.SetWindowSize(windowHandle.WindowHandle, 800, 600);
            Window.Activate();
        }
    }
}
