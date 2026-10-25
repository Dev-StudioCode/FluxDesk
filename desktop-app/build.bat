@echo off
setlocal
echo ========================================================
echo   Building FluxDesk - Windows 11 Virtual Desktop Engine
echo ========================================================

set CSC="C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if not exist %CSC% (
    echo Error: C# compiler not found at %CSC%
    pause
    exit /b 1
)

set WPF_DIR="C:\Windows\Microsoft.NET\Framework64\v4.0.30319\WPF"

%CSC% /nologo /target:winexe /optimize+ /out:FluxDesk.exe ^
  /r:System.dll,System.Core.dll,System.Drawing.dll,System.Windows.Forms.dll,System.Xaml.dll,System.Web.Extensions.dll ^
  /r:%WPF_DIR%\PresentationFramework.dll ^
  /r:%WPF_DIR%\PresentationCore.dll ^
  /r:%WPF_DIR%\WindowsBase.dll ^
  src\NativeMethods.cs ^
  src\VirtualDesktopService.cs ^
  src\ConfigManager.cs ^
  src\WallpaperRotator.cs ^
  src\WallpaperEngineGuard.cs ^
  src\TrayIconManager.cs ^
  src\MainWindow.cs ^
  src\App.cs

if %ERRORLEVEL% EQU 0 (
    echo [SUCCESS] FluxDesk.exe compiled successfully!
) else (
    echo [ERROR] Build failed with error code %ERRORLEVEL%
)

endlocal
