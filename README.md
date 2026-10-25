# ⚡ Project FluxDesk

> **Dynamic Per-Virtual-Desktop Wallpaper Slideshow & Touchpad Preloader for Windows 11**

**FluxDesk** solves the notorious Windows 11 bug where switching virtual desktops with a **4-finger touchpad swipe** flashes a **blank or black screen**, ensuring every virtual workspace has its wallpaper cached and pre-rendered in Desktop Window Manager (DWM) for 60/120fps buttery-smooth swipe transitions.

This repository is split into two complementary implementations:

1. 🖥️ **[`/desktop-app`](desktop-app/)**: The standalone, zero-dependency C# / WPF desktop application with a Fluent dark dashboard, system tray menu, and slideshow engine. (Reference implementation).
2. 🦅 **[`/windhawk-mod`](windhawk-mod/)**: The high-performance native C++ mod injected directly into `explorer.exe` via the [Windhawk](https://windhawk.net/) modding engine for zero RAM overhead and instant shell-level event response.

---

## 🗂️ Repository Architecture

```
FluxDesk/
├── desktop-app/              # Standalone C# .NET Desktop Application
│   ├── src/                  # Source files (WPF, Win32 COM, Slideshow, Config)
│   ├── build.bat             # 1-second standalone build script using csc.exe
│   ├── FluxDesk.exe          # Pre-compiled standalone executable (47 KB)
│   └── LICENSE               # MIT License
│
├── windhawk-mod/             # In-Process C++ Windhawk Mod for explorer.exe
│   ├── FluxDesk.cpp          # Complete Windhawk mod source code
│   ├── README.md             # Windhawk mod setup & compilation guide
│   └── LICENSE               # MIT License
│
├── LICENSE                   # Root MIT License covering the entire repository
└── README.md                 # Project Overview & Architecture Guide
```

---

## ⚖️ Architecture Comparison

| Feature | Desktop Application (`/desktop-app`) | Windhawk Mod (`/windhawk-mod`) |
| :--- | :--- | :--- |
| **Language** | C# (WPF / .NET Framework 4.8) | C++ (MinGW / Clang via Windhawk) |
| **Execution Mode** | Standalone `.exe` in user space | Injected DLL inside `explorer.exe` |
| **UI** | Interactive Fluent Dark Window + Tray | Native Windhawk Mod Settings UI |
| **Event Detection** | COM service query & timer polling | Native `IVirtualDesktopNotification` sink & `Wh_SetFunctionHook` |
| **Memory Footprint** | ~35 MB | **< 1 MB** |
| **Best For** | Visual control, preview cards, easy launch | Zero-overhead, permanent Windows integration |

---

## 🚀 Quick Start

### Option A: Running the Desktop Application
```cmd
cd desktop-app
build.bat
FluxDesk.exe
```

### Option B: Using the Windhawk Mod
1. Open [Windhawk](https://windhawk.net/).
2. Enable Developer mode (`⋮` -> Advanced).
3. Click **"New mod"**, paste the code from [`windhawk-mod/FluxDesk.cpp`](windhawk-mod/FluxDesk.cpp), and click **"Compile Mod"**.
4. Configure your wallpapers directly in Windhawk's Settings tab.

---

## 📜 Licensing

Both the desktop application and the Windhawk mod are licensed under the **[MIT License](LICENSE)**.
- **For Desktop App**: Allows unrestricted usage, sharing, and portfolio demonstration (Project FluxDesk).
- **For Windhawk Mod**: Complies with the official [Windhawk Mods Repository](https://github.com/ramensoftware/windhawk-mods) requirement that all community submissions use the MIT License.
