using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace FluxDesk
{
    public class MainWindow : Window
    {
        private readonly VirtualDesktopService _vdService;
        private readonly WallpaperRotator _rotator;
        private readonly AppConfig _config;
        private readonly TrayIconManager _trayManager;

        private WrapPanel _desktopsPanel;
        private TextBlock _txtFolder;
        private TextBlock _txtStatus;
        private TextBlock _txtTimer;
        private TextBlock _txtDesktopCount;
        private ComboBox _cmbInterval;
        private CheckBox _chkShuffle;
        private CheckBox _chkAutostart;
        private Border _pnlWallpaperEngine;
        private TextBlock _txtWallpaperEngine;
        private DispatcherTimer _uiRefreshTimer;
        private Guid _lastActiveDesktopId = Guid.Empty;

        public MainWindow(
            VirtualDesktopService vdService,
            WallpaperRotator rotator,
            AppConfig config,
            TrayIconManager trayManager)
        {
            _vdService = vdService;
            _rotator = rotator;
            _config = config;
            _trayManager = trayManager;

            InitializeComponent();
            HookEvents();
            RefreshAll();
        }

        private void InitializeComponent()
        {
            Title = "FluxDesk - Virtual Desktop Dynamic Wallpaper Engine";
            Width = 1060;
            Height = 720;
            MinWidth = 900;
            MinHeight = 600;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;

            // Outer Root Border
            Border rootBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(14, 18, 23)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(48, 54, 61)),
                BorderThickness = new Thickness(1.5),
                CornerRadius = new CornerRadius(16),
                Effect = new DropShadowEffect
                {
                    Color = Colors.Black,
                    Direction = 270,
                    ShadowDepth = 8,
                    Opacity = 0.55,
                    BlurRadius = 24
                }
            };

            Grid mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(56) }); // Title Bar
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(85) }); // Stats Cards
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(60) }); // Control Bar
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Desktop Cards Scroll
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // WE / Gesture Info Banner

            // 1. TITLE BAR
            Grid titleBar = CreateTitleBar();
            Grid.SetRow(titleBar, 0);
            mainGrid.Children.Add(titleBar);

            // 2. STATS CARDS
            Grid statsBar = CreateStatsBar();
            Grid.SetRow(statsBar, 1);
            mainGrid.Children.Add(statsBar);

            // 3. CONTROL TOOLBAR
            Grid toolBar = CreateControlToolBar();
            Grid.SetRow(toolBar, 2);
            mainGrid.Children.Add(toolBar);

            // 4. DESKTOPS SCROLLVIEWER
            ScrollViewer scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Padding = new Thickness(24, 8, 24, 12)
            };
            _desktopsPanel = new WrapPanel
            {
                ItemWidth = 230,
                ItemHeight = 240,
                Orientation = Orientation.Horizontal
            };
            scrollViewer.Content = _desktopsPanel;
            Grid.SetRow(scrollViewer, 3);
            mainGrid.Children.Add(scrollViewer);

            // 5. BOTTOM INFO BANNER
            UIElement bottomBanner = CreateBottomBanner();
            Grid.SetRow(bottomBanner, 4);
            mainGrid.Children.Add(bottomBanner);

            rootBorder.Child = mainGrid;
            Content = rootBorder;
        }

        private Grid CreateTitleBar()
        {
            Grid bar = new Grid
            {
                Background = new SolidColorBrush(Color.FromRgb(20, 24, 31))
            };
            bar.MouseLeftButtonDown += (s, e) => { if (e.ButtonState == MouseButtonState.Pressed) DragMove(); };

            // Left side: Logo & Title
            StackPanel leftPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(20, 0, 0, 0)
            };

            // Icon Dot
            Border iconDot = new Border
            {
                Width = 28,
                Height = 28,
                CornerRadius = new CornerRadius(14),
                Background = new LinearGradientBrush(
                    Color.FromRgb(0, 180, 255),
                    Color.FromRgb(180, 50, 255),
                    new Point(0, 0),
                    new Point(1, 1)),
                Margin = new Thickness(0, 0, 12, 0)
            };
            TextBlock iconSymbol = new TextBlock
            {
                Text = "⚡",
                FontSize = 14,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            iconDot.Child = iconSymbol;
            leftPanel.Children.Add(iconDot);

            TextBlock title = new TextBlock
            {
                Text = "FLUXDESK",
                FontWeight = FontWeights.ExtraBold,
                FontSize = 16,
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0)
            };
            leftPanel.Children.Add(title);

            Border pill = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(40, 0, 220, 130)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0, 200, 120)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(8, 2, 8, 2),
                VerticalAlignment = VerticalAlignment.Center
            };
            TextBlock pillText = new TextBlock
            {
                Text = "PRELOADED & ACTIVE",
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(0, 240, 150))
            };
            pill.Child = pillText;
            leftPanel.Children.Add(pill);

            bar.Children.Add(leftPanel);

            // Right side: Window buttons
            StackPanel rightPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 16, 0)
            };

            Button btnMinimize = CreateTitleButton("—", "Minimize to Taskbar");
            btnMinimize.Click += (s, e) => WindowState = WindowState.Minimized;
            rightPanel.Children.Add(btnMinimize);

            Button btnTray = CreateTitleButton("▼", "Minimize to System Tray");
            btnTray.Click += (s, e) =>
            {
                Hide();
                if (_trayManager != null)
                {
                    _trayManager.ShowNotification("FluxDesk Minimized", "FluxDesk continues to rotate and preload your virtual desktop wallpapers in the background.");
                }
            };
            rightPanel.Children.Add(btnTray);

            Button btnClose = CreateTitleButton("✕", "Close (Runs in Tray)");
            btnClose.Click += (s, e) =>
            {
                Hide();
                if (_trayManager != null)
                {
                    _trayManager.ShowNotification("FluxDesk Background Active", "FluxDesk is minimized to your system tray. Touchpad 4-finger gestures will remain preloaded.");
                }
            };
            rightPanel.Children.Add(btnClose);

            bar.Children.Add(rightPanel);
            return bar;
        }

        private Button CreateTitleButton(string text, string tooltip)
        {
            Button btn = new Button
            {
                Content = text,
                ToolTip = tooltip,
                Width = 34,
                Height = 30,
                Margin = new Thickness(4, 0, 0, 0),
                Background = new SolidColorBrush(Color.FromRgb(30, 36, 44)),
                Foreground = Brushes.LightGray,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand,
                FontSize = 12,
                FontWeight = FontWeights.Bold
            };
            return btn;
        }

        private Grid CreateStatsBar()
        {
            Grid grid = new Grid
            {
                Margin = new Thickness(24, 8, 24, 0)
            };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.4, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Card 1: Virtual Desktops
            _txtDesktopCount = new TextBlock { Text = "4 Desktops Active", FontSize = 13, FontWeight = FontWeights.Bold, Foreground = Brushes.White };
            TextBlock lblVDSub = new TextBlock { Text = "Windows 11 Virtual Switching", FontSize = 11, Foreground = Brushes.Gray, Margin = new Thickness(0, 2, 0, 0) };
            grid.Children.Add(CreateStatCard("🖥️ VIRTUAL DESKTOPS", _txtDesktopCount, lblVDSub, 0));

            // Card 2: Folder
            _txtFolder = new TextBlock { Text = "Loading...", FontSize = 12, FontWeight = FontWeights.Bold, Foreground = Brushes.White, TextTrimming = TextTrimming.CharacterEllipsis };
            _txtStatus = new TextBlock { Text = "0 Wallpapers Found", FontSize = 11, Foreground = new SolidColorBrush(Color.FromRgb(0, 190, 255)), Margin = new Thickness(0, 2, 0, 0) };
            grid.Children.Add(CreateStatCard("📁 WALLPAPER SOURCE", _txtFolder, _txtStatus, 1));

            // Card 3: Preload Engine
            TextBlock txtPreload = new TextBlock { Text = "DWM Pre-Rendered", FontSize = 13, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(0, 240, 150)) };
            TextBlock lblPreloadSub = new TextBlock { Text = "Zero Touchpad Blank Screen", FontSize = 11, Foreground = Brushes.Gray, Margin = new Thickness(0, 2, 0, 0) };
            grid.Children.Add(CreateStatCard("⚡ SWITCH ENGINE", txtPreload, lblPreloadSub, 2));

            // Card 4: Timer Countdown
            _txtTimer = new TextBlock { Text = "Next in: 10:00", FontSize = 13, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(240, 180, 50)) };
            TextBlock lblTimerSub = new TextBlock { Text = "Auto Rotation Timer", FontSize = 11, Foreground = Brushes.Gray, Margin = new Thickness(0, 2, 0, 0) };
            grid.Children.Add(CreateStatCard("⏱️ ROTATION", _txtTimer, lblTimerSub, 3));

            return grid;
        }

        private Border CreateStatCard(string header, TextBlock value1, TextBlock value2, int column)
        {
            Border card = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(22, 27, 34)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(40, 46, 54)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(14, 10, 14, 10),
                Margin = new Thickness(column == 0 ? 0 : 8, 0, column == 3 ? 0 : 8, 0)
            };
            Grid.SetColumn(card, column);

            StackPanel sp = new StackPanel();
            TextBlock lblHeader = new TextBlock
            {
                Text = header,
                FontSize = 9.5,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(140, 150, 165)),
                Margin = new Thickness(0, 0, 0, 4)
            };
            sp.Children.Add(lblHeader);
            sp.Children.Add(value1);
            sp.Children.Add(value2);
            card.Child = sp;
            return card;
        }

        private Grid CreateControlToolBar()
        {
            Grid bar = new Grid
            {
                Margin = new Thickness(24, 10, 24, 4)
            };
            bar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Left controls
            bar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Right action buttons

            StackPanel left = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center
            };

            // Folder Browse Button
            Button btnBrowse = new Button
            {
                Content = "📁 Change Folder...",
                Height = 34,
                Padding = new Thickness(12, 0, 12, 0),
                Background = new SolidColorBrush(Color.FromRgb(33, 38, 45)),
                Foreground = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(60, 68, 78)),
                BorderThickness = new Thickness(1),
                Cursor = Cursors.Hand,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 12, 0)
            };
            btnBrowse.Click += BtnBrowse_Click;
            left.Children.Add(btnBrowse);

            // Interval ComboBox
            TextBlock lblInterval = new TextBlock
            {
                Text = "Interval:",
                Foreground = Brushes.LightGray,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(6, 0, 6, 0),
                FontSize = 12
            };
            left.Children.Add(lblInterval);

            _cmbInterval = new ComboBox
            {
                Height = 34,
                Width = 110,
                Margin = new Thickness(0, 0, 14, 0),
                VerticalContentAlignment = VerticalAlignment.Center
            };
            _cmbInterval.Items.Add(new ComboBoxItem { Content = "1 Minute", Tag = 1 });
            _cmbInterval.Items.Add(new ComboBoxItem { Content = "5 Minutes", Tag = 5 });
            _cmbInterval.Items.Add(new ComboBoxItem { Content = "10 Minutes", Tag = 10 });
            _cmbInterval.Items.Add(new ComboBoxItem { Content = "15 Minutes", Tag = 15 });
            _cmbInterval.Items.Add(new ComboBoxItem { Content = "30 Minutes", Tag = 30 });
            _cmbInterval.Items.Add(new ComboBoxItem { Content = "1 Hour", Tag = 60 });
            _cmbInterval.Items.Add(new ComboBoxItem { Content = "Manual", Tag = 0 });
            SetIntervalSelection(_config.IntervalMinutes);
            _cmbInterval.SelectionChanged += CmbInterval_SelectionChanged;
            left.Children.Add(_cmbInterval);

            // Shuffle CheckBox
            _chkShuffle = new CheckBox
            {
                Content = "Shuffle",
                IsChecked = _config.Shuffle,
                Foreground = Brushes.LightGray,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(6, 0, 16, 0),
                FontSize = 12
            };
            _chkShuffle.Checked += (s, e) => _rotator.SetShuffle(true);
            _chkShuffle.Unchecked += (s, e) => _rotator.SetShuffle(false);
            left.Children.Add(_chkShuffle);

            // Autostart CheckBox
            _chkAutostart = new CheckBox
            {
                Content = "Run on Boot",
                IsChecked = _config.StartWithWindows,
                Foreground = Brushes.LightGray,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0),
                FontSize = 12
            };
            _chkAutostart.Checked += (s, e) =>
            {
                _config.StartWithWindows = true;
                ConfigManager.Save(_config);
            };
            _chkAutostart.Unchecked += (s, e) =>
            {
                _config.StartWithWindows = false;
                ConfigManager.Save(_config);
            };
            left.Children.Add(_chkAutostart);

            bar.Children.Add(left);

            // Right side: "Rotate All Now" Button
            StackPanel right = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center
            };

            Button btnRotateAll = new Button
            {
                Content = "⚡ Rotate All Desktops Now",
                Height = 36,
                Padding = new Thickness(16, 0, 16, 0),
                Background = new LinearGradientBrush(
                    Color.FromRgb(0, 150, 240),
                    Color.FromRgb(160, 40, 230),
                    new Point(0, 0),
                    new Point(1, 1)),
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand
            };
            btnRotateAll.Click += (s, e) =>
            {
                _rotator.RotateAll();
                _rotator.ResetTimer();
            };
            right.Children.Add(btnRotateAll);

            Grid.SetColumn(right, 1);
            bar.Children.Add(right);

            return bar;
        }

        private UIElement CreateBottomBanner()
        {
            StackPanel container = new StackPanel
            {
                Margin = new Thickness(24, 0, 24, 16)
            };

            // Touchpad gesture info card
            Border gestureCard = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(30, 0, 180, 255)),
                BorderBrush = new SolidColorBrush(Color.FromArgb(90, 0, 180, 255)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12, 8, 12, 8),
                Margin = new Thickness(0, 0, 0, 6)
            };
            StackPanel spGesture = new StackPanel { Orientation = Orientation.Horizontal };
            spGesture.Children.Add(new TextBlock
            {
                Text = "🖐️  Touchpad 4-Finger Swipe:",
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(0, 220, 255)),
                FontSize = 11.5,
                Margin = new Thickness(0, 0, 8, 0)
            });
            spGesture.Children.Add(new TextBlock
            {
                Text = "Wallpapers for all desktops are preloaded into Windows DWM. Switching virtual desktops will glide smoothly with zero blank screen.",
                Foreground = Brushes.WhiteSmoke,
                FontSize = 11.5
            });
            gestureCard.Child = spGesture;
            container.Children.Add(gestureCard);

            // Wallpaper Engine Banner
            _pnlWallpaperEngine = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(25, 255, 170, 0)),
                BorderBrush = new SolidColorBrush(Color.FromArgb(80, 255, 170, 0)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12, 8, 12, 8),
                Visibility = Visibility.Collapsed
            };
            StackPanel spWE = new StackPanel { Orientation = Orientation.Horizontal };
            spWE.Children.Add(new TextBlock
            {
                Text = "⚙️  Wallpaper Engine Notice:",
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(255, 190, 40)),
                FontSize = 11.5,
                Margin = new Thickness(0, 0, 8, 0)
            });
            _txtWallpaperEngine = new TextBlock
            {
                Foreground = Brushes.WhiteSmoke,
                FontSize = 11.5,
                TextWrapping = TextWrapping.Wrap
            };
            spWE.Children.Add(_txtWallpaperEngine);
            _pnlWallpaperEngine.Child = spWE;
            container.Children.Add(_pnlWallpaperEngine);

            return container;
        }

        private void HookEvents()
        {
            _rotator.OnRotated += () =>
            {
                Dispatcher.Invoke(new Action(RefreshDesktops));
            };

            _rotator.OnTimerTick += (seconds) =>
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    if (_config.IntervalMinutes > 0)
                    {
                        int mins = seconds / 60;
                        int secs = seconds % 60;
                        _txtTimer.Text = string.Format("Next in: {0:D2}:{1:D2}", mins, secs);
                    }
                    else
                    {
                        _txtTimer.Text = "Manual Mode";
                    }
                }));
            };

            // 2-second UI poll to detect virtual desktop switches or count changes
            _uiRefreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            _uiRefreshTimer.Tick += (s, e) =>
            {
                Guid current = _vdService.GetCurrentDesktopId();
                int count = _vdService.GetDesktopCount();

                if (current != _lastActiveDesktopId || count != _desktopsPanel.Children.Count)
                {
                    _lastActiveDesktopId = current;
                    RefreshDesktops();
                    _rotator.EnsureAllDesktopsAssigned();
                }
            };
            _uiRefreshTimer.Start();
        }

        public void RefreshAll()
        {
            _txtFolder.Text = _rotator.CurrentFolder;
            _txtStatus.Text = _rotator.ImageCount + " Wallpapers Loaded";
            RefreshDesktops();
            CheckWallpaperEngine();
        }

        private void CheckWallpaperEngine()
        {
            var status = WallpaperEngineGuard.CheckStatus();
            if (status.IsRunning)
            {
                _pnlWallpaperEngine.Visibility = Visibility.Visible;
                _txtWallpaperEngine.Text = "Wallpaper Engine is running. Tip: Open Wallpaper Engine Settings > General > uncheck 'Override wallpaper image' so Windows native preloading slides without glitching.";
            }
            else
            {
                _pnlWallpaperEngine.Visibility = Visibility.Collapsed;
            }
        }

        private void RefreshDesktops()
        {
            _desktopsPanel.Children.Clear();
            var desktops = _vdService.GetAllDesktops();
            _txtDesktopCount.Text = desktops.Count + " Desktops Active";

            foreach (var dt in desktops)
            {
                _desktopsPanel.Children.Add(CreateDesktopCard(dt));
            }
        }

        private Border CreateDesktopCard(VirtualDesktopInfo dt)
        {
            Border card = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(22, 27, 34)),
                BorderBrush = dt.IsActive
                    ? new SolidColorBrush(Color.FromRgb(0, 220, 255))
                    : new SolidColorBrush(Color.FromRgb(48, 54, 61)),
                BorderThickness = new Thickness(dt.IsActive ? 2 : 1),
                CornerRadius = new CornerRadius(12),
                Margin = new Thickness(6, 6, 6, 6),
                Padding = new Thickness(10)
            };

            if (dt.IsActive)
            {
                card.Effect = new DropShadowEffect
                {
                    Color = Color.FromRgb(0, 200, 255),
                    BlurRadius = 14,
                    ShadowDepth = 0,
                    Opacity = 0.5
                };
            }

            Grid grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(26) }); // Header
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(125) }); // Thumbnail
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(24) }); // Filename
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(32) }); // Action Buttons

            // 1. Header (Desktop Title + Active Badge)
            StackPanel header = new StackPanel { Orientation = Orientation.Horizontal };
            TextBlock title = new TextBlock
            {
                Text = dt.DisplayTitle,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                FontSize = 12.5,
                VerticalAlignment = VerticalAlignment.Center
            };
            header.Children.Add(title);

            if (dt.IsActive)
            {
                Border activePill = new Border
                {
                    Background = new SolidColorBrush(Color.FromArgb(50, 0, 220, 255)),
                    CornerRadius = new CornerRadius(6),
                    Padding = new Thickness(5, 1, 5, 1),
                    Margin = new Thickness(6, 0, 0, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };
                activePill.Child = new TextBlock
                {
                    Text = "ACTIVE",
                    FontSize = 9,
                    FontWeight = FontWeights.ExtraBold,
                    Foreground = new SolidColorBrush(Color.FromRgb(0, 230, 255))
                };
                header.Children.Add(activePill);
            }
            Grid.SetRow(header, 0);
            grid.Children.Add(header);

            // 2. Thumbnail
            Border thumbContainer = new Border
            {
                CornerRadius = new CornerRadius(8),
                ClipToBounds = true,
                Background = new SolidColorBrush(Color.FromRgb(10, 13, 17)),
                Margin = new Thickness(0, 2, 0, 4)
            };

            Image img = new Image
            {
                Stretch = Stretch.UniformToFill,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            if (!string.IsNullOrEmpty(dt.WallpaperPath) && File.Exists(dt.WallpaperPath))
            {
                try
                {
                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    bi.CacheOption = BitmapCacheOption.OnLoad;
                    bi.DecodePixelWidth = 320;
                    bi.UriSource = new Uri(dt.WallpaperPath);
                    bi.EndInit();
                    bi.Freeze();
                    img.Source = bi;
                }
                catch { }
            }
            thumbContainer.Child = img;
            Grid.SetRow(thumbContainer, 1);
            grid.Children.Add(thumbContainer);

            // 3. Filename
            string fileName = !string.IsNullOrEmpty(dt.WallpaperPath) ? Path.GetFileName(dt.WallpaperPath) : "No Wallpaper Assigned";
            TextBlock txtFile = new TextBlock
            {
                Text = fileName,
                FontSize = 10.5,
                Foreground = Brushes.Gray,
                TextTrimming = TextTrimming.CharacterEllipsis,
                ToolTip = dt.WallpaperPath,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetRow(txtFile, 2);
            grid.Children.Add(txtFile);

            // 4. Action Buttons
            Grid btnRow = new Grid();
            btnRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            btnRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            Button btnNext = new Button
            {
                Content = "🎲 Next",
                ToolTip = "Rotate just this desktop's wallpaper",
                Height = 26,
                Margin = new Thickness(0, 2, 3, 0),
                Background = new SolidColorBrush(Color.FromRgb(33, 38, 45)),
                Foreground = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(60, 68, 78)),
                BorderThickness = new Thickness(1),
                FontSize = 11,
                Cursor = Cursors.Hand
            };
            int dtIndex = dt.Index;
            btnNext.Click += (s, e) => _rotator.NextForDesktop(dtIndex);
            Grid.SetColumn(btnNext, 0);
            btnRow.Children.Add(btnNext);

            Button btnSwitch = new Button
            {
                Content = "↗ Switch",
                ToolTip = "Switch directly to this desktop",
                Height = 26,
                Margin = new Thickness(3, 2, 0, 0),
                Background = new SolidColorBrush(Color.FromRgb(33, 38, 45)),
                Foreground = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(60, 68, 78)),
                BorderThickness = new Thickness(1),
                FontSize = 11,
                Cursor = Cursors.Hand
            };
            btnSwitch.Click += (s, e) => _vdService.SwitchToDesktop(dtIndex);
            Grid.SetColumn(btnSwitch, 1);
            btnRow.Children.Add(btnSwitch);

            Grid.SetRow(btnRow, 3);
            grid.Children.Add(btnRow);

            card.Child = grid;
            return card;
        }

        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select Folder Containing Wallpapers for Virtual Desktops",
                ShowNewFolderButton = false
            };

            if (!string.IsNullOrEmpty(_config.WallpaperFolder) && Directory.Exists(_config.WallpaperFolder))
            {
                dialog.SelectedPath = _config.WallpaperFolder;
            }

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _rotator.SetFolder(dialog.SelectedPath);
                RefreshAll();
            }
        }

        private void SetIntervalSelection(int minutes)
        {
            foreach (ComboBoxItem item in _cmbInterval.Items)
            {
                if ((int)item.Tag == minutes)
                {
                    _cmbInterval.SelectedItem = item;
                    return;
                }
            }
            _cmbInterval.SelectedIndex = 2; // default 10 min
        }

        private void CmbInterval_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem selected = _cmbInterval.SelectedItem as ComboBoxItem;
            if (selected != null)
            {
                int min = (int)selected.Tag;
                _rotator.SetInterval(min);
            }
        }
    }
}
