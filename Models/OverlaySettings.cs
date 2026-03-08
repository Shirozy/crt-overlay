using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CrtOverlayApp.Models;

public sealed class OverlaySettings : INotifyPropertyChanged
{
    private double _masterOpacity = 0.58;
    private int _scanlineSpacing = 3;
    private double _scanlineOpacity = 0.12;
    private double _noiseOpacity = 0.08;
    private double _noiseDensity = 0.22;
    private double _vignetteStrength = 0.55;
    private double _tintOpacity = 0.12;
    private byte _tintRed = 125;
    private byte _tintGreen = 255;
    private byte _tintBlue = 210;
    private double _glowStrength = 0.16;
    private double _curvature = 0.30;
    private double _flickerStrength = 0.03;
    private bool _excludeFromCapture = false;

    public event PropertyChangedEventHandler? PropertyChanged;

    public double MasterOpacity { get => _masterOpacity; set => SetField(ref _masterOpacity, value); }
    public int ScanlineSpacing { get => _scanlineSpacing; set => SetField(ref _scanlineSpacing, value); }
    public double ScanlineOpacity { get => _scanlineOpacity; set => SetField(ref _scanlineOpacity, value); }
    public double NoiseOpacity { get => _noiseOpacity; set => SetField(ref _noiseOpacity, value); }
    public double NoiseDensity { get => _noiseDensity; set => SetField(ref _noiseDensity, value); }
    public double VignetteStrength { get => _vignetteStrength; set => SetField(ref _vignetteStrength, value); }
    public double TintOpacity { get => _tintOpacity; set => SetField(ref _tintOpacity, value); }
    public byte TintRed { get => _tintRed; set => SetField(ref _tintRed, value); }
    public byte TintGreen { get => _tintGreen; set => SetField(ref _tintGreen, value); }
    public byte TintBlue { get => _tintBlue; set => SetField(ref _tintBlue, value); }
    public double GlowStrength { get => _glowStrength; set => SetField(ref _glowStrength, value); }
    public double Curvature { get => _curvature; set => SetField(ref _curvature, value); }
    public double FlickerStrength { get => _flickerStrength; set => SetField(ref _flickerStrength, value); }
    public bool ExcludeFromCapture { get => _excludeFromCapture; set => SetField(ref _excludeFromCapture, value); }

    public OverlaySettings Clone() => new()
    {
        MasterOpacity = MasterOpacity,
        ScanlineSpacing = ScanlineSpacing,
        ScanlineOpacity = ScanlineOpacity,
        NoiseOpacity = NoiseOpacity,
        NoiseDensity = NoiseDensity,
        VignetteStrength = VignetteStrength,
        TintOpacity = TintOpacity,
        TintRed = TintRed,
        TintGreen = TintGreen,
        TintBlue = TintBlue,
        GlowStrength = GlowStrength,
        Curvature = Curvature,
        FlickerStrength = FlickerStrength,
        ExcludeFromCapture = ExcludeFromCapture
    };

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
