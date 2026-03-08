using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using CrtOverlayApp.Models;
using CrtOverlayApp.Services;

namespace CrtOverlayApp.Windows;

public partial class OverlayWindow : Window
{
    private const int GwlExStyle = -20;
    private const int WsExTransparent = 0x00000020;
    private const int WsExLayered = 0x00080000;
    private const int WsExToolWindow = 0x00000080;

    private static readonly IntPtr HwndTopmost = new(-1);
    private const uint SwpShowWindow = 0x0040;
    private const uint SwpNoActivate = 0x0010;

    private readonly Rect _screenBoundsPixels;
    private IntPtr _hwnd;

    public OverlayWindow(Rect bounds, OverlaySettings settings)
    {
        InitializeComponent();

        _screenBoundsPixels = bounds;

        Loaded += (_, _) => ApplySettings(settings);
        SourceInitialized += (_, _) => ConfigureWindow();
    }

    public void ApplySettings(OverlaySettings settings)
    {
        OverlayControl.Settings = settings.Clone();
        DisplayAffinityService.Apply(_hwnd, settings.ExcludeFromCapture);
    }

    public void Tick(double deltaSeconds)
    {
        OverlayControl.AdvanceFrame(deltaSeconds);
    }

    private void ConfigureWindow()
    {
        _hwnd = new WindowInteropHelper(this).Handle;
        var style = GetWindowLong(_hwnd, GwlExStyle);
        SetWindowLong(_hwnd, GwlExStyle, style | WsExLayered | WsExTransparent | WsExToolWindow);

        // Important: position in physical pixels so DPI scaling does not inflate the overlay size.
        SetWindowPos(
            _hwnd,
            HwndTopmost,
            (int)_screenBoundsPixels.Left,
            (int)_screenBoundsPixels.Top,
            Math.Max(1, (int)_screenBoundsPixels.Width),
            Math.Max(1, (int)_screenBoundsPixels.Height),
            SwpShowWindow | SwpNoActivate);

        DisplayAffinityService.Apply(_hwnd, OverlayControl.Settings.ExcludeFromCapture);
    }

    [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
    private static extern int GetWindowLong(IntPtr hwnd, int index);

    [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
    private static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(
        IntPtr hWnd,
        IntPtr hWndInsertAfter,
        int x,
        int y,
        int cx,
        int cy,
        uint uFlags);
}
