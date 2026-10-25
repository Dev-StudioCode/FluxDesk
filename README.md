# ⚡ FluxDesk

> **Dynamic Per-Virtual-Desktop Wallpaper Slideshow & Touchpad Preloader for Windows 11**

**FluxDesk** is a lightweight, zero-dependency Windows 11 utility that automatically assigns, preloads, and rotates distinct wallpapers across all your Windows virtual desktops. 

It solves the common Windows 11 bug where switching virtual desktops with a **4-finger touchpad swipe** flashes a **blank/black screen**, ensuring every virtual workspace has its wallpaper cached and pre-rendered in Desktop Window Manager (DWM) for 60/120fps smooth swipe transitions.

---

## ✨ Features

- 🖥️ **Unique Wallpapers Per Virtual Desktop**: Every virtual desktop gets its own distinct background image from your selected wallpaper folder.
- ⚡ **Zero-Blank 4-Finger Touchpad Gestures**: Wallpapers are preloaded into Windows Explorer & DWM via native internal Shell COM interfaces (`IVirtualDesktopManagerInternal::SetDesktopWallpaper`), eliminating black screens and transition lag during touchpad swipes.
- 🔄 **Dynamic Folder Slideshow**: Automatically rotates wallpapers on a configurable interval (1 min, 5 min, 10 min, 15 min, 30 min, 1 hour, or manual mode).
- 📁 **Auto-Detection**: Automatically detects your Windows background slideshow folder or allows selecting any custom pictures folder.
- 🔀 **Smart Duplicate Prevention**: Shuffles and distributes images so no two virtual desktops display the same wallpaper at the same time.
- 🎮 **Wallpaper Engine Compatibility**: Detects active Wallpaper Engine processes and provides guidance to prevent wallpaper override conflicts.
- 🎛️ **Modern Fluent Dark Dashboard**: Real-time thumbnail cards for each virtual desktop, active desktop indicator, one-click manual rotation, and single-desktop re-rolls.
- 🔔 **System Tray Integration**: Runs silently in the background, minimizing to the system tray with quick-access context menu controls.
- 🚀 **Zero External Dependencies**: Pure native C# / WPF compiled into a standalone 47 KB executable using Windows' built-in .NET compiler (`csc.exe`). No Node, Python, or external runtimes required.

---

## 🛠️ Architecture

```
FluxDesk/
├── src/
│   ├── NativeMethods.cs        # Undocumented Windows 11 COM APIs (IVirtualDesktopManagerInternal, IVirtualDesktop)
│   ├── VirtualDesktopService.cs# Virtual desktop enumeration, current desktop tracking, and preloading
│   ├── ConfigManager.cs        # Settings persistence (%APPDATA%\FluxDesk\config.json) & Windows Run autostart
│   ├── WallpaperRotator.cs     # Slideshow engine, image pool scanner, and rotation timer
│   ├── WallpaperEngineGuard.cs # Detects Wallpaper Engine and resolves conflict states
│   ├── TrayIconManager.cs      # System tray icon, custom GDI+ icon rendering, and tray menu
│   ├── MainWindow.cs           # Fluent dark mode WPF interface with live desktop preview cards
│   └── App.cs                  # Entry point, single-instance mutex, and application lifecycle
├── build.bat                   # Instant 1-second build script using native Windows csc.exe
├── FluxDesk.exe                # Pre-compiled standalone executable
├── LICENSE                     # MIT License
└── README.md                   # Documentation
```

---

## 🚀 Quick Start

### Running FluxDesk
1. Double-click `FluxDesk.exe` to launch the dashboard.
2. FluxDesk will automatically detect your active virtual desktops and your current Windows background wallpaper folder (e.g. `CyberPunk HQ`).
3. Distinct wallpapers are immediately preloaded across all virtual desktops.
4. Close or minimize the window (`✕` or `_`) to send FluxDesk to the system tray.

### Preloading & Touchpad Gesture Verification
- Perform a **4-finger horizontal swipe** on your laptop trackpad to switch between virtual desktops.
- Because each desktop's wallpaper was pre-registered into Windows DWM by FluxDesk, the wallpaper slides smoothly in real time with **zero black flash or delay**.

---

## ⚙️ Wallpaper Engine Notice

If you use **Wallpaper Engine**:
- Wallpaper Engine renders live backgrounds only to the currently focused desktop surface. During a Windows 11 4-finger swipe, Windows DWM animates the underlying native desktop wallpapers.
- To prevent Wallpaper Engine from overriding your distinct native wallpapers with a temporary static snapshot:
  1. Open **Wallpaper Engine Settings**.
  2. Go to the **General** tab under the **Windows** section.
  3. Uncheck **`Override wallpaper image`**.
- This allows Wallpaper Engine to run its live wallpaper on the active desktop while FluxDesk guarantees that every other virtual desktop has a preloaded background ready for instant touchpad switching.

---

## 🔨 Building from Source

FluxDesk compiles directly using Windows' built-in .NET Framework compiler without needing Visual Studio or .NET SDK:

```cmd
build.bat
```

Or via PowerShell:
```powershell
& .\build.bat
```

---

## 📜 License

MIT License. Copyright (c) 2026 Dev-StudioCode.
