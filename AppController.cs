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

    private const int HotkeyToggleId = 0x5142;
    private const int HotkeyPreset1Id = 0x5143;
    private const int HotkeyPreset2Id = 0x5144;
    private const int HotkeyIncreaseId = 0x5145;
    private const int HotkeyDecreaseId = 0x5146;
    private const int WmHotkey = 0x0312;
    private const uint ModControl = 0x0002;
    private const uint ModAlt = 0x0001;
    private const uint ModNoRepeat = 0x4000;
    private const uint VkC = 0x43;
    private const uint Vk1 = 0x31;
    private const uint Vk2 = 0x32;
    private const uint VkUp = 0x26;
    private const uint VkDown = 0x28;

    public AppController(Application application)
    {
        _application = application;
        _frameTimer = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromMilliseconds(GetFrameIntervalMs(_settings.TargetFps))
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

        var presets = new ToolStripMenuItem("Presets");
        presets.DropDownItems.Add("Green terminal", null, (_, _) => ApplyPreset(OverlayPresets.GreenTerminal()));
        presets.DropDownItems.Add("Blue monitor", null, (_, _) => ApplyPreset(OverlayPresets.BlueMonitor()));
        presets.DropDownItems.Add("Amber terminal", null, (_, _) => ApplyPreset(OverlayPresets.AmberTerminal()));
        presets.DropDownItems.Add("Sony PVM", null, (_, _) => ApplyPreset(OverlayPresets.SonyPvm()));
        presets.DropDownItems.Add("VHS", null, (_, _) => ApplyPreset(OverlayPresets.Vhs()));
        menu.Items.Add(presets);

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
        var originalSettings = _settings.Clone();
        var window = new SettingsWindow(_settings)
        {
            Owner = _overlayWindows.Count > 0 ? _overlayWindows[0] : null,
            Topmost = true
        };

        window.PreviewSettingsChanged += previewSettings =>
        {
            _settings = previewSettings;
            ApplySettingsToAllWindows();
        };

        if (window.ShowDialog() == true && window.Result is not null)
        {
            _settings = window.Result;
            ApplySettingsToAllWindows();
            SettingsPersistenceService.Save(_settings);
        }
        else
        {
            _settings = originalSettings;
            ApplySettingsToAllWindows();
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
        _frameTimer.Interval = TimeSpan.FromMilliseconds(GetFrameIntervalMs(_settings.TargetFps));

        foreach (var window in _overlayWindows)
        {
            window.ApplySettings(_settings);
        }

        UpdateTrayState();
    }

    private void ApplyPreset(OverlaySettings preset)
    {
        preset.ExcludeFromCapture = _settings.ExcludeFromCapture;
        _settings = preset;
        ApplySettingsToAllWindows();
        SettingsPersistenceService.Save(_settings);
    }

    private void AdjustMasterOpacity(double delta)
    {
        _settings.MasterOpacity = Math.Clamp(_settings.MasterOpacity + delta, 0.05, 1.0);
        ApplySettingsToAllWindows();
        SettingsPersistenceService.Save(_settings);
    }

    private static double GetFrameIntervalMs(int targetFps)
    {
        var fps = Math.Clamp(targetFps, 15, 60);
        return 1000.0 / fps;
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
        RegisterHotKey(_hotkeySource.Handle, HotkeyToggleId, ModControl | ModAlt | ModNoRepeat, VkC);
        RegisterHotKey(_hotkeySource.Handle, HotkeyPreset1Id, ModControl | ModAlt | ModNoRepeat, Vk1);
        RegisterHotKey(_hotkeySource.Handle, HotkeyPreset2Id, ModControl | ModAlt | ModNoRepeat, Vk2);
        RegisterHotKey(_hotkeySource.Handle, HotkeyIncreaseId, ModControl | ModAlt | ModNoRepeat, VkUp);
        RegisterHotKey(_hotkeySource.Handle, HotkeyDecreaseId, ModControl | ModAlt | ModNoRepeat, VkDown);
    }

    private void DestroyHotkeyWindow()
    {
        if (_hotkeySource is null)
        {
            return;
        }

        UnregisterHotKey(_hotkeySource.Handle, HotkeyToggleId);
        UnregisterHotKey(_hotkeySource.Handle, HotkeyPreset1Id);
        UnregisterHotKey(_hotkeySource.Handle, HotkeyPreset2Id);
        UnregisterHotKey(_hotkeySource.Handle, HotkeyIncreaseId);
        UnregisterHotKey(_hotkeySource.Handle, HotkeyDecreaseId);
        _hotkeySource.RemoveHook(WndProc);
        _hotkeySource.Dispose();
        _hotkeySource = null;
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WmHotkey && wParam.ToInt32() == HotkeyToggleId)
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
