using System;
using System.Runtime.InteropServices;

namespace AllMyLights.Platforms.Windows
{
    public class WindowsDesktop: Desktop
    {

        const int SPI_SETDESKWALLPAPER = 0x0014;
        const int UpdateIniFile = 0x01;
        const int SendChangeEvent = 0x02;
        const int FuWinIni = UpdateIniFile | SendChangeEvent;

        public override void SetBackground(string filePath)
        {
           SystemParametersInfo(
                SPI_SETDESKWALLPAPER,
                0,
                filePath,
                FuWinIni
            );
        }

        [DllImport("User32.dll", CharSet = CharSet.Unicode)]
        private static extern int SystemParametersInfo(Int32 uAction, Int32 uParam, String lpvParam, Int32 fuWinIni);
    }
}
