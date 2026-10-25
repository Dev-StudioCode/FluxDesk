using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Threading;

namespace FluxDesk
{
    public class WallpaperRotator
    {
        private readonly VirtualDesktopService _vdService;
        private readonly AppConfig _config;
        private readonly List<string> _images = new List<string>();
        private readonly Random _rand = new Random();
        private readonly DispatcherTimer _timer = new DispatcherTimer();
        private int _currentIndex = 0;
        private int _secondsRemaining = 0;

        public event Action OnRotated;
        public event Action<int> OnTimerTick; // sends seconds remaining

        public WallpaperRotator(VirtualDesktopService vdService, AppConfig config)
        {
            _vdService = vdService;
            _config = config;

            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;

            ScanImages();
            ResetTimer();
        }

        public string CurrentFolder
        {
            get { return _config.WallpaperFolder; }
        }

        public int ImageCount
        {
            get { return _images.Count; }
        }

        public int SecondsRemaining
        {
            get { return _secondsRemaining; }
        }

        public void ScanImages()
        {
            _images.Clear();
            if (!string.IsNullOrEmpty(_config.WallpaperFolder) && Directory.Exists(_config.WallpaperFolder))
            {
                string[] extensions = new string[] { "*.jpg", "*.jpeg", "*.png", "*.bmp", "*.webp" };
                foreach (string ext in extensions)
                {
                    try
                    {
                        string[] files = Directory.GetFiles(_config.WallpaperFolder, ext, SearchOption.AllDirectories);
                        _images.AddRange(files);
                    }
                    catch { }
                }
            }

            // Remove duplicate paths
            var distinct = _images.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            _images.Clear();
            _images.AddRange(distinct);

            if (_config.Shuffle && _images.Count > 1)
            {
                ShuffleList(_images);
            }
            _currentIndex = 0;
        }

        private void ShuffleList(List<string> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int k = _rand.Next(i + 1);
                string temp = list[k];
                list[k] = list[i];
                list[i] = temp;
            }
        }

        public void SetFolder(string folder)
        {
            _config.WallpaperFolder = folder;
            ConfigManager.Save(_config);
            ScanImages();
            RotateAll();
        }

        public void SetInterval(int minutes)
        {
            _config.IntervalMinutes = minutes;
            ConfigManager.Save(_config);
            ResetTimer();
        }

        public void SetShuffle(bool shuffle)
        {
            _config.Shuffle = shuffle;
            ConfigManager.Save(_config);
            if (shuffle && _images.Count > 1)
            {
                ShuffleList(_images);
            }
        }

        public void Start()
        {
            ResetTimer();
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }

        public void ResetTimer()
        {
            if (_config.IntervalMinutes > 0)
            {
                _secondsRemaining = _config.IntervalMinutes * 60;
                if (!_timer.IsEnabled) _timer.Start();
            }
            else
            {
                _secondsRemaining = 0;
                _timer.Stop();
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_config.IntervalMinutes <= 0) return;

            _secondsRemaining--;
            if (OnTimerTick != null)
            {
                OnTimerTick(_secondsRemaining);
            }

            if (_secondsRemaining <= 0)
            {
                RotateAll();
                ResetTimer();
            }
        }

        public void RotateAll()
        {
            if (_images.Count == 0) return;

            int desktopCount = _vdService.GetDesktopCount();
            if (desktopCount == 0) return;

            // Pick distinct images for all virtual desktops
            List<string> selectedImages = new List<string>();

            for (int i = 0; i < desktopCount; i++)
            {
                if (_currentIndex >= _images.Count)
                {
                    _currentIndex = 0;
                    if (_config.Shuffle && _images.Count > 1)
                    {
                        ShuffleList(_images);
                    }
                }

                string img = _images[_currentIndex % _images.Count];
                // Ensure distinct if possible
                int attempts = 0;
                while (selectedImages.Contains(img) && attempts < _images.Count)
                {
                    _currentIndex++;
                    img = _images[_currentIndex % _images.Count];
                    attempts++;
                }

                selectedImages.Add(img);
                _currentIndex++;
            }

            // Apply and preload to Windows Explorer & DWM
            for (int i = 0; i < desktopCount; i++)
            {
                _vdService.SetDesktopWallpaper(i, selectedImages[i]);
            }

            if (OnRotated != null)
            {
                OnRotated();
            }
        }

        public void NextForDesktop(int desktopIndex)
        {
            if (_images.Count == 0) return;

            var desktops = _vdService.GetAllDesktops();
            var used = new HashSet<string>(desktops.Select(d => d.WallpaperPath).Where(p => !string.IsNullOrEmpty(p)), StringComparer.OrdinalIgnoreCase);

            string chosen = null;
            for (int attempt = 0; attempt < _images.Count; attempt++)
            {
                int idx = _rand.Next(_images.Count);
                string candidate = _images[idx];
                if (!used.Contains(candidate))
                {
                    chosen = candidate;
                    break;
                }
            }

            if (string.IsNullOrEmpty(chosen))
            {
                chosen = _images[_rand.Next(_images.Count)];
            }

            _vdService.SetDesktopWallpaper(desktopIndex, chosen);

            if (OnRotated != null)
            {
                OnRotated();
            }
        }

        public void EnsureAllDesktopsAssigned()
        {
            if (_images.Count == 0) return;

            var desktops = _vdService.GetAllDesktops();
            bool modified = false;

            for (int i = 0; i < desktops.Count; i++)
            {
                if (string.IsNullOrEmpty(desktops[i].WallpaperPath) || !File.Exists(desktops[i].WallpaperPath))
                {
                    NextForDesktop(i);
                    modified = true;
                }
            }

            if (modified && OnRotated != null)
            {
                OnRotated();
            }
        }
    }
}
