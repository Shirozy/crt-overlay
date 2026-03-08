using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using MediaColor = System.Windows.Media.Color;
using CrtOverlayApp.Models;

namespace CrtOverlayApp.Windows;

public partial class SettingsWindow : Window, INotifyPropertyChanged
{
    private OverlaySettings? _workingCopy;

    public SettingsWindow(OverlaySettings settings)
    {
        InitializeComponent();
        OriginalSettings = settings.Clone();
        WorkingCopy = settings.Clone();
        DataContext = this;
    }

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
            OnPropertyChanged();
            OnPropertyChanged(nameof(TintPreviewColor));
            RaisePreviewSettingsChanged();
        }
    }

    public MediaColor TintPreviewColor => MediaColor.FromRgb(WorkingCopy.TintRed, WorkingCopy.TintGreen, WorkingCopy.TintBlue);

    public OverlaySettings? Result { get; private set; }

    public event PropertyChangedEventHandler? PropertyChanged;
    public event Action<OverlaySettings>? PreviewSettingsChanged;

    private void Apply_Click(object sender, RoutedEventArgs e)
    {
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
    }

    private void PresetGreen_Click(object sender, RoutedEventArgs e) => SetTint(110, 255, 170);
    private void PresetBlue_Click(object sender, RoutedEventArgs e) => SetTint(110, 185, 255);
    private void PresetRed_Click(object sender, RoutedEventArgs e) => SetTint(255, 110, 110);
    private void PresetAmber_Click(object sender, RoutedEventArgs e) => SetTint(255, 191, 90);
    private void PresetWhite_Click(object sender, RoutedEventArgs e) => SetTint(235, 245, 255);

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

    private void RaisePreviewSettingsChanged()
    {
        PreviewSettingsChanged?.Invoke(WorkingCopy.Clone());
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
