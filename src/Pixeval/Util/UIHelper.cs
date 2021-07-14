using System;
using PInvoke;

namespace Pixeval.Util
{
    // ReSharper disable once InconsistentNaming
    public static class UIHelper
    {
        public static void SetWindowSize(IntPtr hWnd, int width, int height)
        {
            var dpi = User32.GetDpiForWindow(hWnd);
            var scalingFactor = (float) dpi / 96;
            var scaledWidth = (int) (width * scalingFactor);
            var scaledHeight = (int) (height * scalingFactor);

            User32.SetWindowPos(hWnd, User32.SpecialWindowHandles.HWND_TOP,
                0, 0, scaledWidth, scaledHeight,
                User32.SetWindowPosFlags.SWP_NOMOVE);
        }
    }
}