<div align="center">

# FluxDesk Desktop Application
### Standalone Virtual Desktop Dynamic Wallpaper Engine for Windows 11
**Project FluxDesk • Desktop Reference Implementation**

[![Platform: Windows 11](https://img.shields.io/badge/Platform-Windows%2011%20(21H2--25H2)-0078d4?style=flat-square&logo=windows11&logoColor=white)](https://microsoft.com)
[![Language](https://img.shields.io/badge/Language-C%23%20%7C%20WPF-239120?style=flat-square&logo=c-sharp&logoColor=white)](https://docs.microsoft.com/dotnet/csharp/)
[![Runtime](https://img.shields.io/badge/Runtime-.NET%20Framework%204.8-512bd4?style=flat-square&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-success?style=flat-square)](LICENSE)

<br/>

<p align="center">
  <b>A zero-dependency Windows desktop utility that assigns, preloads, and cycles distinct wallpapers across all active virtual desktops with real-time preview cards and system tray controls.</b>
</p>

</div>

---

## Overview

The **FluxDesk Desktop Application** is the standalone reference implementation of Project FluxDesk. Built in C# using WPF and the Windows .NET Framework 4.8 runtime (pre-installed natively on Windows 10 and 11), it requires no third-party package managers, dependencies, or installers.

It continuously coordinates with Windows Desktop Window Manager (DWM) through internal Shell COM interfaces to ensure each virtual desktop maintains a dedicated background texture in memory. This eliminates the black screen lag experienced when switching desktops using trackpad gestures.

---

## Key Features

- **Per-Desktop Preloading**: Each virtual desktop is allocated a unique wallpaper from your selected folder and preloaded into DWM.
- **Dynamic Slideshow**: Automatically rotates wallpapers at user-defined intervals (1, 5, 10, 15, 30, or 60 minutes, or manual mode).
- **Duplicate Prevention**: Shuffles image selection so no two adjacent virtual desktops show identical backgrounds simultaneously.
- **Fluent Dark Dashboard**: Displays live desktop thumbnail preview cards, the active desktop status badge, and individual re-roll buttons.
- **System Tray Integration**: Operates unobtrusively in the background with context menu actions to rotate, toggle shuffle, or pause rotation.
- **Instant Compilation**: Built-in batch script compiles the complete application in under two seconds using Windows' native `csc.exe` compiler.

---

## Architecture

The application is structured into modular service components:

- **`src/NativeMethods.cs`**: Declares internal Windows 11 Shell COM interfaces (`IVirtualDesktopManagerInternal`, `IVirtualDesktop`, `IServiceProvider10`) and Win32 P/Invoke signatures.
- **`src/VirtualDesktopService.cs`**: Handles desktop enumeration, current desktop tracking, and programmatic wallpaper assignment.
- **`src/WallpaperRotator.cs`**: Manages wallpaper indexing, duplicate-free random selection, and countdown timer dispatch.
- **`src/ConfigManager.cs`**: Handles application settings persistence (`%APPDATA%\FluxDesk\config.json`) and Windows Run autostart registry integration.
- **`src/WallpaperEngineGuard.cs`**: Detects running Wallpaper Engine processes and provides collision prevention guidance.
- **`src/TrayIconManager.cs`**: Manages the system tray icon, custom GDI+ icon rendering, and context menu events.
- **`src/MainWindow.cs`**: Fluent dark mode WPF interface displaying live desktop cards and controls.
- **`src/App.cs`**: Application entry point with single-instance mutex enforcement and lifecycle management.

---

## Building from Source

The project can be built without Visual Studio or the .NET SDK using the native Windows Framework compiler:

```cmd
build.bat
```

This compiles all source files into a standalone `FluxDesk.exe` binary in the current directory.

---

## License

This software is released under the **MIT License**. See [LICENSE](LICENSE) for details.
