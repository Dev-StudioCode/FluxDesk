<div align="center">

# Virtual Desktop Wallpaper Switch
### Native Virtual Desktop Wallpaper Coordination & Preloading Engine for Windows 11 (24H2+)
**Project FluxDesk - Windhawk Native Mod Edition**

[![Windhawk Engine](https://img.shields.io/badge/Engine-Windhawk%20v1.4%2B-7952b3?style=flat-square&logo=visual-studio-code&logoColor=white)](https://windhawk.net/)
[![Target Process](https://img.shields.io/badge/Target-explorer.exe-0078d4?style=flat-square&logo=windows&logoColor=white)](https://microsoft.com/windows)
[![Windows 11 Compatible](https://img.shields.io/badge/OS-Windows%2011%2024H2%2B-00a4ef?style=flat-square&logo=windows11&logoColor=white)](https://microsoft.com)
[![Language](https://img.shields.io/badge/Language-C%2B%2B20-00599c?style=flat-square&logo=c%2B%2B&logoColor=white)](https://en.cppreference.com/)
[![Windhawk Users](https://windhawk-badges.luizbizzio.com/mod/virtual-desktop-wallpaper-switch/users)](https://windhawk.net/mods/virtual-desktop-wallpaper-switch)
[![Windhawk Rating](https://windhawk-badges.luizbizzio.com/mod/virtual-desktop-wallpaper-switch/rating)](https://windhawk.net/mods/virtual-desktop-wallpaper-switch)
[![Windhawk Version](https://windhawk-badges.luizbizzio.com/mod/virtual-desktop-wallpaper-switch/version)](https://windhawk.net/mods/virtual-desktop-wallpaper-switch)
[![License: MIT](https://img.shields.io/badge/License-MIT-success?style=flat-square)](LICENSE)

<br/>

> Published on the Windhawk Marketplace via [ramensoftware/windhawk-mods#7733](https://github.com/ramensoftware/windhawk-mods/pull/7733).

<br/>

<p align="center">
  <b>A lightweight Windhawk mod for Windows 11 (24H2+) that assigns distinct wallpapers across virtual desktops, preloads them for smooth touchpad transitions, and backs up original wallpapers in persistent storage.</b>
</p>

</div>

---

## Overview

Under Windows 11 Version 24H2 (Build 26100+), Windows natively supports distinct per-desktop wallpapers through the internal Shell virtual desktop manager (`IVirtualDesktopManagerInternal`). When wallpapers are properly assigned to virtual desktops ahead of time, Windows preloads the desktop surfaces so that touchpad gestures (such as 3- and 4-finger swipes) transition smoothly between workspaces.

**Virtual Desktop Wallpaper Switch** runs inside `explorer.exe` on a dedicated worker thread. It reads images from a configured folder (or explicit per-desktop paths) and assigns them to each virtual desktop. It also listens for desktop creation and destruction events via `IVirtualDesktopNotificationService` to automatically synchronize wallpapers as workspaces change.

---

## Architecture & Storage Management

### Windows Shell Integration
The mod communicates with the Windows 11 Immersive Shell via COM:
* **`CLSID_ImmersiveShell`**: Obtains shell service providers.
* **`IVirtualDesktopManagerInternal`**: Enumerates virtual desktops (`GetDesktops`) and assigns individual backgrounds (`SetDesktopWallpaper`).
* **`IVirtualDesktopNotificationService`**: Registers a non-intrusive event sink (`IVirtualDesktopNotification`) to handle desktop creation and deletion dynamically without polling.

### System Changes and Persistence
* **What Windows Stores**: Calling `SetDesktopWallpaper` updates the per-desktop background in Windows. Windows stores these paths in the user registry under:
  `HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\VirtualDesktops\Desktops\{GUID}`
* **Persistent Backup**: Before overwriting any desktop wallpaper, the mod queries the existing wallpaper path and saves it to Windhawk persistent mod storage, keyed by the desktop's unique GUID (`Wh_SetStringValue`). This ensures the original wallpaper is remembered across Explorer restarts, sign-outs, and system reboots.
* **Restoration on Unload**: When `restoreOriginalOnUnload` is enabled (default), unloading or disabling the mod restores the backed-up wallpapers to each desktop and removes the backup keys from storage (`Wh_DeleteValue`). If this option is turned off, the assigned wallpapers remain saved in your Windows settings.
* **Dynamic Workspace Updates**: When a desktop loses its assignment (e.g. settings cleared or reduced), its original wallpaper is restored from storage.

---

## Configuration Settings

The mod provides the following settings in the Windhawk interface:

* **Wallpaper Folder Path (`wallpaperFolder`)**: Full directory path containing `.jpg`, `.jpeg`, `.png`, `.bmp`, or `.webp` image files.
* **Desktop 1-4 Wallpaper (`desktop1Wallpaper` - `desktop4Wallpaper`)**: Optional static image paths to assign to specific desktop numbers.
* **Shuffle Folder Wallpapers (`shuffle`)**: Randomizes wallpaper assignments from the folder across active desktops on initialization and settings reloads.
* **Restore Original Wallpapers on Exit (`restoreOriginalOnUnload`)**: Restores original desktop backgrounds from Windhawk persistent storage when the mod is disabled or unloaded.

---

## Wallpaper Engine Compatibility Note

If running alongside **Wallpaper Engine**:
* Wallpaper Engine renders exclusively to the active foreground desktop layer. During a touchpad swipe, Windows animates the underlying native shell wallpapers.
* To prevent Wallpaper Engine from overriding native per-desktop backgrounds:
  1. Open **Wallpaper Engine Settings**.
  2. Navigate to **General** -> **Windows**.
  3. Uncheck **`Override wallpaper image`**.
* This allows Wallpaper Engine to animate on your active desktop while Windows maintains distinct backgrounds across adjacent virtual workspaces.

---

## License

This mod is licensed under the **MIT License**.
