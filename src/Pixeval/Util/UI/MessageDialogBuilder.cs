using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.Interop;
using Pixeval.Pages.Misc;
using WinRT;

namespace Pixeval.Util.UI
{
	public static class MessageDialogBuilder
	{
		public static async Task<bool> CreateOkCancel(string title, string content)
		{
			var result = false;
			var messageDialog = new MessageDialog(content, title)
			{
				DefaultCommandIndex = 1,
				CancelCommandIndex = 1,
				Options = MessageDialogOptions.AcceptUserInputAfterDelay
			}.InitializeWithWindow();
			messageDialog.Commands.Add(new UICommand(MessageContentDialogResources.OkButtonContent, _ => result = true));
			messageDialog.Commands.Add(new UICommand(MessageContentDialogResources.CancelButtonContent, _ => result = false));
			_ = await messageDialog.ShowAsync();
			return result;
		}

		public static async Task CreateAcknowledgement(string title, string content)
		{
			var messageDialog = new MessageDialog(content, title)
			{
				DefaultCommandIndex = 0,
				CancelCommandIndex = 0,
				Options = MessageDialogOptions.AcceptUserInputAfterDelay
			}.InitializeWithWindow();
			messageDialog.Commands.Add(new UICommand(MessageContentDialogResources.OkButtonContent, _ => { }));
			_ = await messageDialog.ShowAsync();
		}


		private static T InitializeWithWindow<T>(this T obj)
		{
			if (Window.Current is null)
				obj.As<IInitializeWithWindow>()?.Initialize(GetWindow.HWnd); //HWnd 或者 User32.GetActiveWindow()
			return obj;
		}

		[ComImport, Guid("3E68D4BD-7135-4D10-8018-9FB6D9F33FA1"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		private interface IInitializeWithWindow
		{
			void Initialize([In] IntPtr hWnd);
		}
	}
}