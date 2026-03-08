using System;
using System.Runtime.InteropServices;

namespace CrtOverlayApp.Services;

public static class DisplayAffinityService
{
    private const uint WDA_NONE = 0x00000000;
    private const uint WDA_EXCLUDEFROMCAPTURE = 0x00000011;

    public static void Apply(IntPtr hwnd, bool excludeFromCapture)
    {
        if (hwnd == IntPtr.Zero)
        {
            return;
        }

        try
        {
            SetWindowDisplayAffinity(hwnd, excludeFromCapture ? WDA_EXCLUDEFROMCAPTURE : WDA_NONE);
        }
        catch
        {
            // Ignore unsupported configurations.
        }
    }

    [DllImport("user32.dll")]
    private static extern bool SetWindowDisplayAffinity(IntPtr hWnd, uint dwAffinity);
}
