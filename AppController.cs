using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Threading;
using CrtOverlayApp.Models;
using CrtOverlayApp.Windows;
using CrtOverlayApp.Services;
using Application = System.Windows.Application;

namespace CrtOverlayApp;

public sealed class AppController : IDisposable
{
    private readonly Application _application;
    private readonly List<OverlayWindow> _overlayWindows = new();
    private readonly DispatcherTimer _frameTimer;
    private readonly NotifyIcon _notifyIcon;
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

    private HwndSource? _hotkeySource;
    private OverlaySettings _settings = SettingsPersistenceService.Load();
    private bool _isEnabled = true;

    private const int HotkeyId = 0x5142;
    private const int WmHotkey = 0x0312;
    private const uint ModControl = 0x0002;
    private const uint ModAlt = 0x0001;
    private const uint ModNoRepeat = 0x4000;
    private const uint VkC = 0x43;

    public AppController(Application application)
    {
        _application = application;
        _frameTimer = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromMilliseconds(33)
        };
        _frameTimer.Tick += (_, _) => RenderFrame();

        _notifyIcon = new NotifyIcon
        {
            Text = "CRT Overlay",
            Visible = true,
            ContextMenuStrip = BuildMenu(),
            Icon = System.Drawing.SystemIcons.Application
        };
        _notifyIcon.DoubleClick += (_, _) => ToggleOverlay();
    }

    public void Start()
    {
        CreateHotkeyWindow();
        CreateOverlayWindows();
        UpdateTrayState();
        _frameTimer.Start();
    }

    public void Dispose()
    {
        _frameTimer.Stop();
        DestroyHotkeyWindow();

        foreach (var window in _overlayWindows)
        {
            window.Close();
        }

        _overlayWindows.Clear();
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
    }

    private void CreateOverlayWindows()
    {
        DestroyOverlayWindows();

        foreach (Screen screen in Screen.AllScreens)
        {
            var bounds = screen.Bounds;
            var window = new OverlayWindow(new Rect(bounds.X, bounds.Y, bounds.Width, bounds.Height), _settings);
            _overlayWindows.Add(window);
            window.Show();
        }
    }

    private void DestroyOverlayWindows()
    {
        foreach (var window in _overlayWindows)
        {
            window.Close();
        }

        _overlayWindows.Clear();
    }

    private void RenderFrame()
    {
        if (!_isEnabled)
        {
            return;
        }

        var delta = _stopwatch.Elapsed.TotalSeconds;
        _stopwatch.Restart();

        foreach (var window in _overlayWindows)
        {
            window.Tick(delta);
        }
    }

    private ContextMenuStrip BuildMenu()
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add("Toggle overlay", null, (_, _) => ToggleOverlay());
        menu.Items.Add("Settings", null, (_, _) => ShowSettings());
        menu.Items.Add("Refresh monitors", null, (_, _) => RefreshMonitors());
        menu.Items.Add("Exit", null, (_, _) => _application.Shutdown());
        return menu;
    }

    private void ToggleOverlay()
    {
        _isEnabled = !_isEnabled;
        foreach (var window in _overlayWindows)
        {
            window.Visibility = _isEnabled ? Visibility.Visible : Visibility.Hidden;
        }

        _stopwatch.Restart();
        UpdateTrayState();
    }

    private void ShowSettings()
    {
        var window = new SettingsWindow(_settings)
        {
            Owner = _overlayWindows.Count > 0 ? _overlayWindows[0] : null,
            Topmost = true
        };

        if (window.ShowDialog() == true && window.Result is not null)
        {
            _settings = window.Result;
            ApplySettingsToAllWindows();
            SettingsPersistenceService.Save(_settings);
        }
    }

    private void RefreshMonitors()
    {
        CreateOverlayWindows();
        if (!_isEnabled)
        {
            foreach (var window in _overlayWindows)
            {
                window.Visibility = Visibility.Hidden;
            }
        }
    }

    private void ApplySettingsToAllWindows()
    {
        foreach (var window in _overlayWindows)
        {
            window.ApplySettings(_settings);
        }
    }

    private void UpdateTrayState()
    {
        var status = _isEnabled ? "enabled" : "disabled";
        _notifyIcon.Text = $"CRT Overlay ({status}) - Ctrl+Alt+C toggles";
    }

    private void CreateHotkeyWindow()
    {
        var parameters = new HwndSourceParameters("CrtOverlayHotkeySink")
        {
            Width = 0,
            Height = 0,
            WindowStyle = 0
        };

        _hotkeySource = new HwndSource(parameters);
        _hotkeySource.AddHook(WndProc);
        RegisterHotKey(_hotkeySource.Handle, HotkeyId, ModControl | ModAlt | ModNoRepeat, VkC);
    }

    private void DestroyHotkeyWindow()
    {
        if (_hotkeySource is null)
        {
            return;
        }

        UnregisterHotKey(_hotkeySource.Handle, HotkeyId);
        _hotkeySource.RemoveHook(WndProc);
        _hotkeySource.Dispose();
        _hotkeySource = null;
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WmHotkey && wParam.ToInt32() == HotkeyId)
        {
            ToggleOverlay();
            handled = true;
        }

        return IntPtr.Zero;
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}
