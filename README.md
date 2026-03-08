# CRT Overlay App

A lightweight Windows WPF app that draws a fake CRT effect over every monitor by using transparent, click-through, topmost overlay windows.

## Features

- One overlay window per monitor
- Click-through transparent overlay
- Scanlines, vignette, tint, flicker, RGB mask, and animated noise
- System tray icon
- Global hotkey: **Ctrl + Alt + C**
- Live settings window
- Optional "exclude from capture" flag for supported Windows versions

## Requirements

- Windows 10 or Windows 11
- .NET 8 SDK
- Visual Studio 2022 or the `dotnet` CLI

## Build with Visual Studio

1. Open `CrtOverlayApp.csproj` in Visual Studio 2022.
2. Restore packages if prompted.
3. Set configuration to **Release**.
4. Build the project.
5. Run the produced `CrtOverlayApp.exe`.

## Build with the .NET CLI

Open a terminal in the project folder and run:

```powershell
dotnet restore
dotnet build -c Release
```

The executable will be under:

```text
bin\Release\net8.0-windows\
```

## Run

```powershell
dotnet run -c Release
```

## Usage

- Launch the app.
- The overlay immediately appears on all monitors.
- Press **Ctrl + Alt + C** to toggle the overlay.
- Right-click the tray icon for settings, monitor refresh, or exit.
- Double-click the tray icon to toggle the effect.

## Notes

- This app does **not** capture and post-process the real desktop image. It draws CRT artifacts over the desktop and apps underneath.
- The optional capture exclusion uses a Windows API that is supported for `WDA_EXCLUDEFROMCAPTURE` on Windows 10 version 2004 and later.
- Because the overlay is click-through, it cannot be interacted with directly. Use the tray icon or hotkey.