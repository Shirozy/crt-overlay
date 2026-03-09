using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using CrtOverlayApp.Models;
using MediaColor = System.Windows.Media.Color;

namespace CrtOverlayApp.Windows;

public partial class SettingsWindow : Window, INotifyPropertyChanged
{
    private OverlaySettings? _workingCopy;
    private string _hotkeyStatus = "Press Apply to save changes.";

    public SettingsWindow(OverlaySettings settings)
    {
        InitializeComponent();
        AvailablePresets = new ObservableCollection<PresetOption>(OverlayPresets.All);
        OriginalSettings = settings.Clone();
        WorkingCopy = settings.Clone();
        DataContext = this;
    }

    public ObservableCollection<PresetOption> AvailablePresets { get; }

    public OverlaySettings OriginalSettings { get; }

    public OverlaySettings WorkingCopy
    {
        get => _workingCopy!;
        set
        {
            if (_workingCopy is not null)
            {
                DetachWorkingCopyEvents(_workingCopy);
            }

            _workingCopy = value;
            AttachWorkingCopyEvents(_workingCopy);
            EnsurePresetSelectionDefaults();
            OnPropertyChanged();
            OnPropertyChanged(nameof(TintPreviewColor));
            RaisePreviewSettingsChanged();
        }
    }

    public string HotkeyStatus
    {
        get => _hotkeyStatus;
        set
        {
            _hotkeyStatus = value;
            OnPropertyChanged();
        }
    }

    public MediaColor TintPreviewColor => MediaColor.FromRgb(WorkingCopy.TintRed, WorkingCopy.TintGreen, WorkingCopy.TintBlue);

    public OverlaySettings? Result { get; private set; }

    public event PropertyChangedEventHandler? PropertyChanged;
    public event Action<OverlaySettings>? PreviewSettingsChanged;

    private void Apply_Click(object sender, RoutedEventArgs e)
    {
        NormalizeHotkeys();
        Result = WorkingCopy.Clone();
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        Result = null;
        DialogResult = false;
        Close();
    }

    private void Reset_Click(object sender, RoutedEventArgs e)
    {
        WorkingCopy = new OverlaySettings();
        HotkeyStatus = "Reset to defaults.";
    }

    private void PresetGreen_Click(object sender, RoutedEventArgs e) => SetTint(110, 255, 170);
    private void PresetBlue_Click(object sender, RoutedEventArgs e) => SetTint(110, 185, 255);
    private void PresetRed_Click(object sender, RoutedEventArgs e) => SetTint(255, 110, 110);
    private void PresetAmber_Click(object sender, RoutedEventArgs e) => SetTint(255, 191, 90);
    private void PresetWhite_Click(object sender, RoutedEventArgs e) => SetTint(235, 245, 255);

    private void PresetGreenTerminal_Click(object sender, RoutedEventArgs e) => ApplyPreset("Green Terminal");
    private void PresetBlueMonitor_Click(object sender, RoutedEventArgs e) => ApplyPreset("Blue Monitor");
    private void PresetAmberTerminal_Click(object sender, RoutedEventArgs e) => ApplyPreset("Amber Terminal");
    private void PresetSonyPvm_Click(object sender, RoutedEventArgs e) => ApplyPreset("Sony PVM");
    private void PresetVhs_Click(object sender, RoutedEventArgs e) => ApplyPreset("VHS");
    private void PresetArcade_Click(object sender, RoutedEventArgs e) => ApplyPreset("Arcade");
    private void PresetBroadcast_Click(object sender, RoutedEventArgs e) => ApplyPreset("Broadcast");
    private void PresetSubtle_Click(object sender, RoutedEventArgs e) => ApplyPreset("Subtle CRT");

    private void ApplyPreset(string name)
    {
        var updated = OverlayPresets.CreateByName(name);
        updated.ToggleHotkey = WorkingCopy.ToggleHotkey;
        updated.Preset1Hotkey = WorkingCopy.Preset1Hotkey;
        updated.Preset2Hotkey = WorkingCopy.Preset2Hotkey;
        updated.IncreaseHotkey = WorkingCopy.IncreaseHotkey;
        updated.DecreaseHotkey = WorkingCopy.DecreaseHotkey;
        updated.Preset1Name = WorkingCopy.Preset1Name;
        updated.Preset2Name = WorkingCopy.Preset2Name;
        updated.ExcludeFromCapture = WorkingCopy.ExcludeFromCapture;
        WorkingCopy = updated;
        HotkeyStatus = $"Applied preset: {name}";
    }

    private void SetTint(byte red, byte green, byte blue)
    {
        WorkingCopy.TintRed = red;
        WorkingCopy.TintGreen = green;
        WorkingCopy.TintBlue = blue;
        OnPropertyChanged(nameof(TintPreviewColor));
        RaisePreviewSettingsChanged();
    }

    private void AttachWorkingCopyEvents(OverlaySettings workingCopy)
    {
        if (workingCopy is INotifyPropertyChanged notifyPropertyChanged)
        {
            notifyPropertyChanged.PropertyChanged += WorkingCopy_PropertyChanged;
        }
    }

    private void DetachWorkingCopyEvents(OverlaySettings workingCopy)
    {
        if (workingCopy is INotifyPropertyChanged notifyPropertyChanged)
        {
            notifyPropertyChanged.PropertyChanged -= WorkingCopy_PropertyChanged;
        }
    }

    private void WorkingCopy_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(OverlaySettings.TintRed) or nameof(OverlaySettings.TintGreen) or nameof(OverlaySettings.TintBlue))
        {
            OnPropertyChanged(nameof(TintPreviewColor));
        }

        RaisePreviewSettingsChanged();
    }

    private void NormalizeHotkeys()
    {
        WorkingCopy.ToggleHotkey = NormalizeHotkey(WorkingCopy.ToggleHotkey, "Ctrl+Alt+C");
        WorkingCopy.Preset1Hotkey = NormalizeHotkey(WorkingCopy.Preset1Hotkey, "Ctrl+Alt+1");
        WorkingCopy.Preset2Hotkey = NormalizeHotkey(WorkingCopy.Preset2Hotkey, "Ctrl+Alt+2");
        WorkingCopy.IncreaseHotkey = NormalizeHotkey(WorkingCopy.IncreaseHotkey, "Ctrl+Alt+Up");
        WorkingCopy.DecreaseHotkey = NormalizeHotkey(WorkingCopy.DecreaseHotkey, "Ctrl+Alt+Down");
        EnsurePresetSelectionDefaults();
        HotkeyStatus = "Hotkeys normalized and ready to save.";
    }

    private void EnsurePresetSelectionDefaults()
    {
        if (!AvailablePresets.Any(x => x.Name == WorkingCopy.Preset1Name))
        {
            WorkingCopy.Preset1Name = "Sony PVM";
        }

        if (!AvailablePresets.Any(x => x.Name == WorkingCopy.Preset2Name))
        {
            WorkingCopy.Preset2Name = "VHS";
        }
    }

    private static string NormalizeHotkey(string? value, string fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        var parts = value!
            .Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(x => char.ToUpperInvariant(x[0]) + x.Substring(1).ToLowerInvariant())
            .ToList();

        if (parts.Count == 0)
        {
            return fallback;
        }

        return string.Join("+", parts);
    }

    private void RaisePreviewSettingsChanged()
    {
        PreviewSettingsChanged?.Invoke(WorkingCopy.Clone());
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
