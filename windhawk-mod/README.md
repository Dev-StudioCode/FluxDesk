# 🦅 Windhawk Mod: Virtual Desktop Wallpaper Switch (FluxDesk)

> **In-Process Virtual Desktop Wallpaper Switcher & Touchpad Transition Preloader for Windows 11**

This directory contains the native C++ Windhawk mod implementation of **Project FluxDesk**.

Unlike external desktop applications that must poll or run continuous background loops, this mod is injected directly into **`explorer.exe`** by the [Windhawk](https://windhawk.net/) engine. It hooks directly into Windows Shell internal COM interfaces to preload and switch wallpapers seamlessly across all virtual desktops.

---

## 🚀 Key Advantages

| Feature | External Desktop App (`/desktop-app`) | Windhawk Mod (`/windhawk-mod`) |
| :--- | :--- | :--- |
| **Execution Context** | Out-of-process standalone `.exe` | In-process directly inside `explorer.exe` |
| **RAM Footprint** | ~35 MB (.NET runtime + WPF) | **< 1 MB** (zero extra processes) |
| **Switch Latency** | Inter-process COM dispatch | **Immediate / zero latency** |
| **Touchpad Gestures** | Preloaded via COM proxy | Preloaded directly within Explorer/DWM memory |
| **Installation** | Run standalone executable | One-click via Windhawk UI |

---

## 📁 File Structure

- **`FluxDesk.cpp`**: Primary single-file Windhawk mod source code containing the official metadata block, settings schema, COM declarations, notification listener, and lifecycle hooks (`Wh_ModInit`, `Wh_ModSettingsChanged`, `Wh_ModUninit`).
- **`LICENSE`**: MIT License covering this mod for the official Windhawk mod repository.

---

## 🛠️ How to Test & Install in Windhawk

1. Install [Windhawk](https://windhawk.net/) (v1.4 or higher).
2. Open the Windhawk application window.
3. Click the three dots (`⋮`) in the top right and select **"Developer channel"** / **"Advanced"** to enable mod development mode.
4. Click **"New mod"** in the top navigation bar.
5. In the mod source editor, replace all default code with the contents of [`FluxDesk.cpp`](FluxDesk.cpp).
6. Click **"Compile Mod"** (or press `Ctrl+S`).
7. Windhawk compiles the C++ code using its bundled MinGW/Clang toolchain and injects it directly into `explorer.exe`.
8. Open the mod's **Settings** tab in Windhawk to configure your wallpaper folder or per-desktop images!

---

## 📜 Windows APIs & Mechanisms Used

1. **`CLSID_ImmersiveShell` (`{C2F03A33-21F5-47FA-B4BB-156362A2F239}`)**:
   - Queries `IServiceProvider` to locate internal shell services.
2. **`IVirtualDesktopManagerInternal` (`{53F5CA0B-158F-4124-900C-057158060B27}`)**:
   - `SetDesktopWallpaper(IVirtualDesktop*, HSTRING)`: Sets and caches the wallpaper on the specific virtual desktop in DWM, eliminating black flicker on touchpad swipes.
   - `GetDesktops(IObjectArray**)`: Enumerates all active virtual workspaces.
   - `SwitchDesktopWithAnimation`: Intercepted via `Wh_SetFunctionHook` for instant wallpaper synchronization.
3. **`IVirtualDesktopNotificationService` (`{0CD45E71-D927-4F15-8B0A-8FEF525337BF}`)**:
   - `Register(IVirtualDesktopNotification*, DWORD*)`: Subscribes our listener to all shell virtual desktop events (`CurrentVirtualDesktopChanged`, `VirtualDesktopCreated`, `VirtualDesktopDestroyed`).
4. **`IDesktopWallpaper` / `SystemParametersInfoW`**:
   - Active desktop fallback to ensure desktop surface refresh across all multi-monitor setups.
