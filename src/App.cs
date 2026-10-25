using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;

namespace FluxDesk
{
    public class App : Application
    {
        private static Mutex _mutex;
        private TrayIconManager _trayManager;
        private VirtualDesktopService _vdService;
        private WallpaperRotator _rotator;
        private AppConfig _config;
        private MainWindow _mainWindow;

        public App()
        {
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        }

        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                bool createdNew;
                _mutex = new Mutex(true, "FluxDesk_SingleInstance_Mutex", out createdNew);

                if (!createdNew)
                {
                    MessageBox.Show("FluxDesk is already running in your system tray.", "FluxDesk", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                App app = new App();
                app.Execute(args);
            }
            catch (Exception ex)
            {
                File.WriteAllText("crash.log", ex.ToString());
                MessageBox.Show("FluxDesk Startup Error: " + ex.Message, "FluxDesk Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Execute(string[] args)
        {
            // 1. Initialize services
            _vdService = new VirtualDesktopService();
            _config = ConfigManager.Load();
            _rotator = new WallpaperRotator(_vdService, _config);

            // 2. Initialize System Tray
            _trayManager = new TrayIconManager(
                onOpenWindow: () =>
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        if (_mainWindow == null)
                        {
                            _mainWindow = new MainWindow(_vdService, _rotator, _config, _trayManager);
                        }
                        _mainWindow.Show();
                        _mainWindow.WindowState = WindowState.Normal;
                        _mainWindow.Activate();
                        _mainWindow.RefreshAll();
                    }));
                },
                onRotateNow: () =>
                {
                    _rotator.RotateAll();
                    _rotator.ResetTimer();
                },
                onToggleShuffle: () =>
                {
                    _rotator.SetShuffle(!_config.Shuffle);
                    _rotator.RotateAll();
                },
                onTogglePause: () =>
                {
                    if (_config.IntervalMinutes > 0)
                    {
                        _rotator.SetInterval(0);
                    }
                    else
                    {
                        _rotator.SetInterval(10);
                    }
                },
                onExit: () =>
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        if (_trayManager != null) _trayManager.Dispose();
                        ConfigManager.Save(_config);
                        Shutdown();
                    }));
                }
            );

            // 3. Create Main Window
            _mainWindow = new MainWindow(_vdService, _rotator, _config, _trayManager);

            // 4. Start rotation & preloading
            _rotator.Start();
            _rotator.EnsureAllDesktopsAssigned();

            // 5. Check arguments
            bool startMinimized = args != null && args.Any(a => a.Equals("--minimized", StringComparison.OrdinalIgnoreCase));

            if (!startMinimized)
            {
                _mainWindow.Show();
                _mainWindow.Activate();
            }
            else
            {
                _trayManager.ShowNotification("FluxDesk Active", "FluxDesk is running in the background. All virtual desktop wallpapers are preloaded.");
            }

            Run(_mainWindow);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (_trayManager != null)
            {
                _trayManager.Dispose();
            }
            if (_mutex != null)
            {
                _mutex.ReleaseMutex();
                _mutex.Dispose();
            }
            base.OnExit(e);
        }
    }
}
