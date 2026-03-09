using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Threading;
using CrtOverlayApp.Models;
using CrtOverlayApp.Services;
using CrtOverlayApp.Windows;
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
    private const uint ModAlt = 0x0001;
    private const uint ModControl = 0x0002;
    private const uint ModShift = 0x0004;
    private const uint ModWin = 0x0008;
    private const uint ModNoRepeat = 0x4000;

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
        foreach (var preset in OverlayPresets.All)
        {
            presets.DropDownItems.Add(preset.DisplayName, null, (_, _) => ApplyPreset(OverlayPresets.CreateByName(preset.Name)));
        }

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
            RebindHotkeys();
        }
        else
        {
            _settings = originalSettings;
            ApplySettingsToAllWindows();
            RebindHotkeys();
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

    private void ApplyPreset(OverlaySettings preset)
    {
        preset.ExcludeFromCapture = _settings.ExcludeFromCapture;
        preset.ToggleHotkey = _settings.ToggleHotkey;
        preset.Preset1Hotkey = _settings.Preset1Hotkey;
        preset.Preset2Hotkey = _settings.Preset2Hotkey;
        preset.IncreaseHotkey = _settings.IncreaseHotkey;
        preset.DecreaseHotkey = _settings.DecreaseHotkey;
        preset.Preset1Name = _settings.Preset1Name;
        preset.Preset2Name = _settings.Preset2Name;
        _settings = preset;
        ApplySettingsToAllWindows();
        SettingsPersistenceService.Save(_settings);
        RebindHotkeys();
    }

    private void AdjustMasterOpacity(double delta)
    {
        _settings.MasterOpacity = Math.Clamp(_settings.MasterOpacity + delta, 0.05, 1.0);
        ApplySettingsToAllWindows();
        SettingsPersistenceService.Save(_settings);
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

    private void UpdateTrayState()
    {
        var status = _isEnabled ? "enabled" : "disabled";
        _notifyIcon.Text = $"CRT Overlay ({status})";
    }

    private static double GetFrameIntervalMs(int targetFps)
    {
        var fps = Math.Clamp(targetFps, 15, 60);
        return 1000.0 / fps;
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
        RebindHotkeys();
    }

    private void RebindHotkeys()
    {
        if (_hotkeySource is null)
        {
            return;
        }

        UnregisterAllHotkeys();

        RegisterParsedHotkey(HotkeyToggleId, _settings.ToggleHotkey, "Ctrl+Alt+C");
        RegisterParsedHotkey(HotkeyPreset1Id, _settings.Preset1Hotkey, "Ctrl+Alt+1");
        RegisterParsedHotkey(HotkeyPreset2Id, _settings.Preset2Hotkey, "Ctrl+Alt+2");
        RegisterParsedHotkey(HotkeyIncreaseId, _settings.IncreaseHotkey, "Ctrl+Alt+Up");
        RegisterParsedHotkey(HotkeyDecreaseId, _settings.DecreaseHotkey, "Ctrl+Alt+Down");
    }

    private void RegisterParsedHotkey(int id, string configured, string fallback)
    {
        if (_hotkeySource is null)
        {
            return;
        }

        if (!TryParseHotkey(configured, out var modifiers, out var vk))
        {
            TryParseHotkey(fallback, out modifiers, out vk);
        }

        RegisterHotKey(_hotkeySource.Handle, id, modifiers | ModNoRepeat, vk);
    }

    private static bool TryParseHotkey(string value, out uint modifiers, out uint vk)
    {
        modifiers = 0;
        vk = 0;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var parts = value.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var raw in parts)
        {
            var part = raw.Trim().ToUpperInvariant();
            switch (part)
            {
                case "CTRL":
                case "CONTROL":
                    modifiers |= ModControl;
                    break;
                case "ALT":
                    modifiers |= ModAlt;
                    break;
                case "SHIFT":
                    modifiers |= ModShift;
                    break;
                case "WIN":
                case "WINDOWS":
                    modifiers |= ModWin;
                    break;
                case "UP":
                    vk = 0x26;
                    break;
                case "DOWN":
                    vk = 0x28;
                    break;
                case "LEFT":
                    vk = 0x25;
                    break;
                case "RIGHT":
                    vk = 0x27;
                    break;
                default:
                    if (part.Length == 1 && char.IsLetterOrDigit(part[0]))
                    {
                        vk = part[0];
                    }
                    else if (part.StartsWith("F", StringComparison.Ordinal) &&
                             int.TryParse(part[1..], NumberStyles.Integer, CultureInfo.InvariantCulture, out var fn) &&
                             fn >= 1 && fn <= 24)
                    {
                        vk = (uint)(0x70 + (fn - 1));
                    }
                    break;
            }
        }

        return modifiers != 0 && vk != 0;
    }

    private void DestroyHotkeyWindow()
    {
        if (_hotkeySource is null)
        {
            return;
        }

        UnregisterAllHotkeys();
        _hotkeySource.RemoveHook(WndProc);
        _hotkeySource.Dispose();
        _hotkeySource = null;
    }

    private void UnregisterAllHotkeys()
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
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WmHotkey)
        {
            switch (wParam.ToInt32())
            {
                case HotkeyToggleId:
                    ToggleOverlay();
                    handled = true;
                    break;
                case HotkeyPreset1Id:
                    ApplyPreset(OverlayPresets.CreateByName(_settings.Preset1Name));
                    handled = true;
                    break;
                case HotkeyPreset2Id:
                    ApplyPreset(OverlayPresets.CreateByName(_settings.Preset2Name));
                    handled = true;
                    break;
                case HotkeyIncreaseId:
                    AdjustMasterOpacity(0.03);
                    handled = true;
                    break;
                case HotkeyDecreaseId:
                    AdjustMasterOpacity(-0.03);
                    handled = true;
                    break;
            }
        }

        return IntPtr.Zero;
    }

    [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}
