using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace FluxDesk
{
    public class WallpaperEngineStatus
    {
        public bool IsRunning { get; set; }
        public bool IsOverriding { get; set; }
        public string StatusMessage { get; set; }
        public string Recommendation { get; set; }
    }

    public static class WallpaperEngineGuard
    {
        public static WallpaperEngineStatus CheckStatus()
        {
            var status = new WallpaperEngineStatus();
            try
            {
                var processes = Process.GetProcesses();
                var weProcesses = processes.Where(p =>
                {
                    try
                    {
                        string name = p.ProcessName.ToLower();
                        return name.Contains("wallpaper32") || name.Contains("wallpaper64") || name.Contains("wallpaperservice");
                    }
                    catch { return false; }
                }).ToList();

                status.IsRunning = weProcesses.Count > 0;

                // Check if current desktop wallpaper has WallpaperEngineOverride in path
                string winThemeDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    @"Microsoft\Windows\Themes");

                if (Directory.Exists(winThemeDir))
                {
                    var overrideFiles = Directory.GetFiles(winThemeDir, "WallpaperEngineOverride*");
                    if (overrideFiles.Length > 0)
                    {
                        status.IsOverriding = true;
                    }
                }

                if (status.IsRunning)
                {
                    status.StatusMessage = "Wallpaper Engine Active";
                    status.Recommendation = "To prevent black screens during 4-finger touchpad gestures, open Wallpaper Engine Settings > General > uncheck 'Override wallpaper image'. This allows Windows to smoothly slide your preloaded wallpapers.";
                }
                else
                {
                    status.StatusMessage = "Wallpaper Engine Not Running";
                    status.Recommendation = "Native Windows Virtual Desktop preloading is fully active and optimized.";
                }
            }
            catch (Exception ex)
            {
                status.StatusMessage = "Check error: " + ex.Message;
            }

            return status;
        }
    }
}
