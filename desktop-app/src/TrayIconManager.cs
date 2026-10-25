using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace FluxDesk
{
    public class TrayIconManager : IDisposable
    {
        private NotifyIcon _notifyIcon;
        private readonly Action _onOpenWindow;
        private readonly Action _onRotateNow;
        private readonly Action _onToggleShuffle;
        private readonly Action _onTogglePause;
        private readonly Action _onExit;

        public TrayIconManager(
            Action onOpenWindow,
            Action onRotateNow,
            Action onToggleShuffle,
            Action onTogglePause,
            Action onExit)
        {
            _onOpenWindow = onOpenWindow;
            _onRotateNow = onRotateNow;
            _onToggleShuffle = onToggleShuffle;
            _onTogglePause = onTogglePause;
            _onExit = onExit;

            InitializeTray();
        }

        private void InitializeTray()
        {
            _notifyIcon = new NotifyIcon();
            _notifyIcon.Text = "FluxDesk - Virtual Desktop Wallpapers";
            _notifyIcon.Icon = GenerateTrayIcon();
            _notifyIcon.Visible = true;

            // Context Menu
            ContextMenu menu = new ContextMenu();

            MenuItem header = new MenuItem("FluxDesk - Dashboard");
            header.DefaultItem = true;
            header.Click += (s, e) => _onOpenWindow();
            menu.MenuItems.Add(header);

            menu.MenuItems.Add("-");

            MenuItem rotateItem = new MenuItem("Rotate Wallpapers Now", (s, e) => _onRotateNow());
            menu.MenuItems.Add(rotateItem);

            MenuItem shuffleItem = new MenuItem("Shuffle Mode", (s, e) => _onToggleShuffle());
            menu.MenuItems.Add(shuffleItem);

            MenuItem pauseItem = new MenuItem("Pause / Resume Timer", (s, e) => _onTogglePause());
            menu.MenuItems.Add(pauseItem);

            menu.MenuItems.Add("-");

            MenuItem exitItem = new MenuItem("Exit FluxDesk", (s, e) => _onExit());
            menu.MenuItems.Add(exitItem);

            _notifyIcon.ContextMenu = menu;
            _notifyIcon.DoubleClick += (s, e) => _onOpenWindow();
            _notifyIcon.Click += (s, e) =>
            {
                var me = e as MouseEventArgs;
                if (me != null && me.Button == MouseButtons.Left)
                {
                    _onOpenWindow();
                }
            };
        }

        private Icon GenerateTrayIcon()
        {
            using (Bitmap bmp = new Bitmap(32, 32))
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                // Draw stylish glowing layers icon
                using (LinearGradientBrush bgBrush = new LinearGradientBrush(
                    new Rectangle(0, 0, 32, 32),
                    Color.FromArgb(10, 160, 240),
                    Color.FromArgb(170, 40, 240),
                    LinearGradientMode.ForwardDiagonal))
                {
                    g.FillEllipse(bgBrush, 2, 2, 28, 28);
                }

                // Draw desktop frames inside
                using (Pen pen = new Pen(Color.White, 2f))
                {
                    g.DrawRectangle(pen, 7, 7, 18, 12);
                    g.DrawLine(pen, 11, 23, 21, 23);
                    g.DrawLine(pen, 16, 19, 16, 23);
                }

                IntPtr hIcon = bmp.GetHicon();
                return Icon.FromHandle(hIcon);
            }
        }

        public void ShowNotification(string title, string message)
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.ShowBalloonTip(3000, title, message, ToolTipIcon.Info);
            }
        }

        public void Dispose()
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
                _notifyIcon = null;
            }
        }
    }
}
