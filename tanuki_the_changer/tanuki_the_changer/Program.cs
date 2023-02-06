using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace tanuki_the_changer
{
    class Program
    {
        const int SetDesktopWallpaper = 20;
        const int UpdateIniFile = 0x01;
        const int SendWinIniChange = 0x02;

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        static void Main(string[] args)
        {
            string wallpaperPath = "C:\\Users\\miche\\source\\repos\\tanuki_the_changer\\tanuki_the_changer\\tanuki_wallpaper.jpg";
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
            key.SetValue(@"WallpaperStyle", "2");
            key.SetValue(@"TileWallpaper", "0");
            SystemParametersInfo(SetDesktopWallpaper, 0, wallpaperPath, UpdateIniFile | SendWinIniChange);
        }
    }
}
