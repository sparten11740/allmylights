#if Windows
using System;
using System.Runtime.InteropServices;


namespace AllMyLights.Platforms.Windows
{
    public static class ConsoleWindow
    {
        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);


        public static void Show(bool visible)
        {
            IntPtr hWnd = FindWindow(null, Console.Title);

            if (hWnd != IntPtr.Zero)
            {
                if (visible) ShowWindow(hWnd, 1);
                else ShowWindow(hWnd, 0);
            }
        }
    }
}
#endif