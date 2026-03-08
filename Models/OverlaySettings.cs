using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CrtOverlayApp.Models;

public sealed class OverlaySettings : INotifyPropertyChanged
{
    private double _masterOpacity = 0.58;
    private int _scanlineSpacing = 3;
    private double _scanlineOpacity = 0.12;
    private double _scanlineSoftness = 0.55;
    private double _noiseOpacity = 0.08;
    private double _noiseDensity = 0.22;
    private double _vignetteStrength = 0.55;
    private double _tintOpacity = 0.12;
    private byte _tintRed = 125;
    private byte _tintGreen = 255;
    private byte _tintBlue = 210;
    private double _glowStrength = 0.16;
    private double _flickerStrength = 0.03;
    private bool _excludeFromCapture = false;
    private double _phosphorStrength = 0.12;
    private double _phosphorScale = 1.0;
    private int _maskType = 0;
    private double _horizontalBloom = 0.18;
    private double _gamma = 1.0;
    private double _contrast = 1.0;
    private double _brightness = 0.0;
    private double _screenJitter = 0.15;
    private double _reflectionStrength = 0.08;
    private int _targetFps = 30;

    public event PropertyChangedEventHandler? PropertyChanged;

    public double MasterOpacity { get => _masterOpacity; set => SetField(ref _masterOpacity, value); }
    public int ScanlineSpacing { get => _scanlineSpacing; set => SetField(ref _scanlineSpacing, value); }
    public double ScanlineOpacity { get => _scanlineOpacity; set => SetField(ref _scanlineOpacity, value); }
    public double ScanlineSoftness { get => _scanlineSoftness; set => SetField(ref _scanlineSoftness, value); }
    public double NoiseOpacity { get => _noiseOpacity; set => SetField(ref _noiseOpacity, value); }
    public double NoiseDensity { get => _noiseDensity; set => SetField(ref _noiseDensity, value); }
    public double VignetteStrength { get => _vignetteStrength; set => SetField(ref _vignetteStrength, value); }
    public double TintOpacity { get => _tintOpacity; set => SetField(ref _tintOpacity, value); }
    public byte TintRed { get => _tintRed; set => SetField(ref _tintRed, value); }
    public byte TintGreen { get => _tintGreen; set => SetField(ref _tintGreen, value); }
    public byte TintBlue { get => _tintBlue; set => SetField(ref _tintBlue, value); }
    public double GlowStrength { get => _glowStrength; set => SetField(ref _glowStrength, value); }
    public double FlickerStrength { get => _flickerStrength; set => SetField(ref _flickerStrength, value); }
    public bool ExcludeFromCapture { get => _excludeFromCapture; set => SetField(ref _excludeFromCapture, value); }
    public double PhosphorStrength { get => _phosphorStrength; set => SetField(ref _phosphorStrength, value); }
    public double PhosphorScale { get => _phosphorScale; set => SetField(ref _phosphorScale, value); }
    public int MaskType { get => _maskType; set => SetField(ref _maskType, value); }
    public double HorizontalBloom { get => _horizontalBloom; set => SetField(ref _horizontalBloom, value); }
    public double Gamma { get => _gamma; set => SetField(ref _gamma, value); }
    public double Contrast { get => _contrast; set => SetField(ref _contrast, value); }
    public double Brightness { get => _brightness; set => SetField(ref _brightness, value); }
    public double ScreenJitter { get => _screenJitter; set => SetField(ref _screenJitter, value); }
    public double ReflectionStrength { get => _reflectionStrength; set => SetField(ref _reflectionStrength, value); }
    public int TargetFps { get => _targetFps; set => SetField(ref _targetFps, value); }

    public OverlaySettings Clone() => new()
    {
        MasterOpacity = MasterOpacity,
        ScanlineSpacing = ScanlineSpacing,
        ScanlineOpacity = ScanlineOpacity,
        ScanlineSoftness = ScanlineSoftness,
        NoiseOpacity = NoiseOpacity,
        NoiseDensity = NoiseDensity,
        VignetteStrength = VignetteStrength,
        TintOpacity = TintOpacity,
        TintRed = TintRed,
        TintGreen = TintGreen,
        TintBlue = TintBlue,
        GlowStrength = GlowStrength,
        FlickerStrength = FlickerStrength,
        ExcludeFromCapture = ExcludeFromCapture,
        PhosphorStrength = PhosphorStrength,
        PhosphorScale = PhosphorScale,
        MaskType = MaskType,
        HorizontalBloom = HorizontalBloom,
        Gamma = Gamma,
        Contrast = Contrast,
        Brightness = Brightness,
        ScreenJitter = ScreenJitter,
        ReflectionStrength = ReflectionStrength,
        TargetFps = TargetFps
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
