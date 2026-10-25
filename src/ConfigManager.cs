using System;
using System.IO;
using System.Web.Script.Serialization;
using Microsoft.Win32;

namespace FluxDesk
{
    public class AppConfig
    {
        public string WallpaperFolder { get; set; }
        public int IntervalMinutes { get; set; }
        public bool Shuffle { get; set; }
        public bool StartWithWindows { get; set; }
        public bool EnableGuard { get; set; }

        public AppConfig()
        {
            IntervalMinutes = 10;
            Shuffle = true;
            StartWithWindows = false;
            EnableGuard = true;
            WallpaperFolder = "";
        }
    }

    public static class ConfigManager
    {
        private static readonly string ConfigDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FluxDesk");
        private static readonly string ConfigPath = Path.Combine(ConfigDir, "config.json");
        private const string RunRegistryKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string AppName = "FluxDesk";

        public static AppConfig Load()
        {
            AppConfig config = null;
            try
            {
                if (File.Exists(ConfigPath))
                {
                    string json = File.ReadAllText(ConfigPath);
                    var jss = new JavaScriptSerializer();
                    config = jss.Deserialize<AppConfig>(json);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Config load error: " + ex.Message);
            }

            if (config == null)
            {
                config = new AppConfig();
            }

            if (string.IsNullOrEmpty(config.WallpaperFolder) || !Directory.Exists(config.WallpaperFolder))
            {
                config.WallpaperFolder = DetectDefaultWallpaperFolder();
            }

            config.StartWithWindows = IsRunOnStartupEnabled();
            return config;
        }

        public static void Save(AppConfig config)
        {
            try
            {
                if (!Directory.Exists(ConfigDir))
                {
                    Directory.CreateDirectory(ConfigDir);
                }
                var jss = new JavaScriptSerializer();
                string json = jss.Serialize(config);
                File.WriteAllText(ConfigPath, json);

                SetRunOnStartup(config.StartWithWindows);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Config save error: " + ex.Message);
            }
        }

        public static string DetectDefaultWallpaperFolder()
        {
            // 1. Check Windows Slideshow or Wallpapers registry
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Wallpapers"))
                {
                    if (key != null)
                    {
                        object curWp = key.GetValue("CurrentWallpaperPath");
                        if (curWp != null)
                        {
                            string dir = Path.GetDirectoryName(curWp.ToString());
                            if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir) && HasImages(dir))
                            {
                                return dir;
                            }
                        }
                    }
                }
            }
            catch { }

            // 2. Check Downloads\CyberPunk HQ
            try
            {
                string downloadsCyber = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", "CyberPunk HQ");
                if (Directory.Exists(downloadsCyber) && HasImages(downloadsCyber))
                {
                    return downloadsCyber;
                }
            }
            catch { }

            // 3. Check Pictures
            try
            {
                string pics = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                if (Directory.Exists(pics) && HasImages(pics))
                {
                    return pics;
                }
            }
            catch { }

            // 4. Fallback to Windows Web Wallpapers
            string winWallpapers = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Web", "Wallpaper");
            if (Directory.Exists(winWallpapers))
            {
                return winWallpapers;
            }

            return Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        }

        private static bool HasImages(string folder)
        {
            try
            {
                string[] exts = new string[] { "*.jpg", "*.jpeg", "*.png", "*.bmp", "*.webp" };
                foreach (string ext in exts)
                {
                    if (Directory.GetFiles(folder, ext).Length > 0)
                        return true;
                }
            }
            catch { }
            return false;
        }

        public static bool IsRunOnStartupEnabled()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunRegistryKey, false))
                {
                    if (key != null)
                    {
                        return key.GetValue(AppName) != null;
                    }
                }
            }
            catch { }
            return false;
        }

        public static void SetRunOnStartup(bool enable)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunRegistryKey, true))
                {
                    if (key != null)
                    {
                        if (enable)
                        {
                            string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                            key.SetValue(AppName, "\"" + exePath + "\" --minimized");
                        }
                        else
                        {
                            key.DeleteValue(AppName, false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("SetRunOnStartup error: " + ex.Message);
            }
        }
    }
}
