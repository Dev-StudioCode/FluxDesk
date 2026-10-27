<div align="center">

# Project FluxDesk
### Dynamic Per-Virtual-Desktop Wallpaper Slideshow & Touchpad Preloader for Windows 11

[![Platform: Windows 11](https://img.shields.io/badge/Platform-Windows%2011%20(21H2--25H2)-0078d4?style=flat-square&logo=windows11&logoColor=white)](https://microsoft.com)
[![Implementations](https://img.shields.io/badge/Implementations-Desktop%20App%20%7C%20Windhawk%20Mod-success?style=flat-square)](https://github.com)
[![Languages](https://img.shields.io/badge/Languages-C%23%20%7C%20C%2B%2B20-blue?style=flat-square)](https://github.com)
[![Windhawk Ready](https://img.shields.io/badge/Windhawk-Ready-7952b3?style=flat-square&logo=visual-studio-code&logoColor=white)](https://windhawk.net/)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg?style=flat-square)](LICENSE)

<br/>

<p align="center">
  <b>A comprehensive solution engineered to eliminate black screen flicker during 3- and 4-finger touchpad transitions on Windows 11 by pre-rendering and caching distinct wallpapers directly inside Desktop Window Manager (DWM).</b>
</p>

</div>

---

## 📌 Overview

Under Windows 11, the **Desktop Window Manager (DWM)** performs real-time interactive sliding animations when switching virtual workspaces via trackpad gestures (such as 3- or 4-finger horizontal swipes). However, when wallpapers are applied globally or switched *after* the transition event occurs, the incoming virtual desktop displays an unrendered **black or blank screen** during the swipe.

**Project FluxDesk** eliminates this latency by pre-registering unique wallpapers for each virtual workspace directly through Windows internal Shell COM interfaces (`IVirtualDesktopManagerInternal::SetDesktopWallpaper`).

This repository provides two distinct, production-ready implementations tailored to different deployment needs:

1. 🖥️ **[`/desktop-app`](desktop-app/)**: Standalone, zero-dependency C# / WPF desktop application featuring an interactive Fluent dark dashboard, real-time desktop preview cards, system tray integration, and an automatic slideshow engine.
2. 🦅 **[`/windhawk-mod`](windhawk-mod/)**: High-performance native C++ mod injected directly into `explorer.exe` via [Windhawk](https://windhawk.net/) for zero RAM overhead (< 1 MB), sub-millisecond in-process event handling, and permanent shell integration.

---

## 🗂️ Repository Architecture

```
FluxDesk/
├── desktop-app/              # Standalone C# .NET Desktop Application
│   ├── src/                  # WPF UI, Win32 COM Interop, Slideshow Engine, Config Manager
│   ├── build.bat             # 1-second standalone build script using native csc.exe
│   ├── FluxDesk.exe          # Pre-compiled standalone binary (47 KB, zero dependencies)
│   ├── README.md             # Desktop app technical documentation
│   └── LICENSE               # MIT License
│
├── windhawk-mod/             # In-Process C++ Windhawk Mod for explorer.exe
│   ├── virtual-desktop-wallpaper-switch.cpp  # Complete Windhawk mod source code
│   ├── windhawk_api.h        # Windhawk API reference header
│   ├── README.md             # Windhawk mod architecture & compilation guide
│   └── LICENSE               # MIT License
│
├── LICENSE                   # Root MIT License covering the entire repository
└── README.md                 # Project Overview & Architecture Guide
```

---

## ⚖️ Architectural Comparison

| Dimension | Desktop Application (`/desktop-app`) | Windhawk Mod (`/windhawk-mod`) |
| :--- | :--- | :--- |
| **Language** | C# (WPF / .NET Framework 4.8) | C++20 (MinGW / Clang via Windhawk) |
| **Execution Domain** | Standalone user-space process (`.exe`) | Injected dynamic module inside `explorer.exe` |
| **User Interface** | Interactive Fluent Dark Window + Tray | Native Windhawk Mod Settings UI |
| **Event Detection** | COM service queries & timer polling | Native `IVirtualDesktopNotification` sink & function hooks |
| **Memory Footprint** | ~35 MB (CLR + UI thread) | **< 1 MB** (zero extra processes) |
| **Touchpad Gestures** | Preloaded via external COM calls | **Direct internal Explorer/DWM texture cache** |
| **Best For** | Visual management, thumbnail previews, manual controls | Zero-overhead, permanent Windows integration |

---

## 🚀 Quick Start

### Option A: Running the Desktop Application
The desktop app requires no installation or external runtimes:
```cmd
cd desktop-app
build.bat
FluxDesk.exe
```

### Option B: Installing the Windhawk Mod
1. Install [Windhawk](https://windhawk.net/) (v1.4+).
2. Enable Developer mode (`⋮` menu $\rightarrow$ **Advanced Settings** $\rightarrow$ Developer channel).
3. Click **"New mod"**, paste the code from [`windhawk-mod/virtual-desktop-wallpaper-switch.cpp`](windhawk-mod/virtual-desktop-wallpaper-switch.cpp), and click **"Compile Mod"**.
4. Configure your wallpapers and slideshow options directly in the mod's **Settings** tab.

---

## 🛡️ Wallpaper Engine Compatibility

If you run **Wallpaper Engine** alongside FluxDesk:
- Wallpaper Engine renders exclusively to the active foreground desktop layer. During a 4-finger touchpad swipe, Windows DWM animates the underlying native shell wallpapers.
- To prevent Wallpaper Engine from overriding native virtual desktop textures:
  1. Open **Wallpaper Engine Settings**.
  2. Navigate to **General** $\rightarrow$ **Windows**.
  3. Uncheck **`Override wallpaper image`**.
- This enables Wallpaper Engine on your active workspace while allowing FluxDesk to keep all adjacent virtual desktop backgrounds permanently pre-rendered and tear-free.

---

## 📜 Licensing

Both implementations are licensed under the **[MIT License](LICENSE)**:
- **Desktop Application**: Allows unrestricted commercial and personal usage, modification, and portfolio demonstration.
- **Windhawk Mod**: Complies with the official [Windhawk Mods Repository](https://github.com/ramensoftware/windhawk-mods) requirement that all community submissions adopt the MIT License.
