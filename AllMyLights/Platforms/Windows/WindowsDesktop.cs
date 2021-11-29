using AllMyLights.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AllMyLights.Platforms.Windows
{
    public class WindowsDesktop : Desktop
    { 

        const int ActionSetWallpaper = 0x0014;
        const int UpdateIniFile = 0x01;
        const int SendChangeEvent = 0x02;
        const int FuWinIni = UpdateIniFile | SendChangeEvent;

        private readonly IDesktopWallpaper Wallpaper = (IDesktopWallpaper) new DesktopWallpaper();

        public override void SetBackground(string filePath)
        {
            SystemParametersInfo(
                 ActionSetWallpaper,
                 0,
                 filePath,
                 FuWinIni
             );
        }

        public override void SetBackgrounds(Dictionary<int, string> filePathByScreen) {
            foreach(var (screen, filePath) in filePathByScreen) {
                var monitorID = Wallpaper.GetMonitorDevicePathAt((uint)screen);
                Wallpaper.SetWallpaper(monitorID, filePath);
            }
        }

        [DllImport("User32.dll", CharSet = CharSet.Unicode)]
        private static extern int SystemParametersInfo(Int32 uAction, Int32 uParam, String lpvParam, Int32 fuWinIni);

        public override IEnumerable<int> GetScreens() => Enumerable.Range(0, (int)Wallpaper.GetMonitorDevicePathCount() - 1);
    }
}
