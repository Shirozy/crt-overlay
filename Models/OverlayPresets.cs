namespace CrtOverlayApp.Models;

public static class OverlayPresets
{
    public static OverlaySettings GreenTerminal() => new()
    {
        MasterOpacity = 0.62, ScanlineSpacing = 3, ScanlineOpacity = 0.16, ScanlineSoftness = 0.60,
        NoiseOpacity = 0.06, NoiseDensity = 0.18, GlowStrength = 0.18, HorizontalBloom = 0.16,
        VignetteStrength = 0.48, TintOpacity = 0.18, TintRed = 100, TintGreen = 255, TintBlue = 165,
        Gamma = 1.05, Contrast = 1.08, Brightness = 0.01, PhosphorStrength = 0.12, PhosphorScale = 1.0,
        MaskType = 0, FlickerStrength = 0.025, ScreenJitter = 0.08, ReflectionStrength = 0.05, TargetFps = 30
    };

    public static OverlaySettings BlueMonitor() => new()
    {
        MasterOpacity = 0.58, ScanlineSpacing = 3, ScanlineOpacity = 0.12, ScanlineSoftness = 0.56,
        NoiseOpacity = 0.07, NoiseDensity = 0.20, GlowStrength = 0.18, HorizontalBloom = 0.18,
        VignetteStrength = 0.52, TintOpacity = 0.14, TintRed = 120, TintGreen = 190, TintBlue = 255,
        Gamma = 1.0, Contrast = 1.02, Brightness = 0.0, PhosphorStrength = 0.11, PhosphorScale = 1.0,
        MaskType = 0, FlickerStrength = 0.03, ScreenJitter = 0.10, ReflectionStrength = 0.08, TargetFps = 30
    };

    public static OverlaySettings AmberTerminal() => new()
    {
        MasterOpacity = 0.64, ScanlineSpacing = 3, ScanlineOpacity = 0.17, ScanlineSoftness = 0.62,
        NoiseOpacity = 0.05, NoiseDensity = 0.18, GlowStrength = 0.20, HorizontalBloom = 0.22,
        VignetteStrength = 0.46, TintOpacity = 0.18, TintRed = 255, TintGreen = 190, TintBlue = 85,
        Gamma = 1.06, Contrast = 1.10, Brightness = 0.02, PhosphorStrength = 0.13, PhosphorScale = 1.0,
        MaskType = 1, FlickerStrength = 0.02, ScreenJitter = 0.08, ReflectionStrength = 0.04, TargetFps = 30
    };

    public static OverlaySettings SonyPvm() => new()
    {
        MasterOpacity = 0.55, ScanlineSpacing = 2, ScanlineOpacity = 0.15, ScanlineSoftness = 0.66,
        NoiseOpacity = 0.04, NoiseDensity = 0.15, GlowStrength = 0.11, HorizontalBloom = 0.12,
        VignetteStrength = 0.35, TintOpacity = 0.08, TintRed = 230, TintGreen = 240, TintBlue = 255,
        Gamma = 0.98, Contrast = 1.12, Brightness = 0.0, PhosphorStrength = 0.18, PhosphorScale = 1.0,
        MaskType = 2, FlickerStrength = 0.015, ScreenJitter = 0.04, ReflectionStrength = 0.03, TargetFps = 60
    };

    public static OverlaySettings Vhs() => new()
    {
        MasterOpacity = 0.60, ScanlineSpacing = 4, ScanlineOpacity = 0.13, ScanlineSoftness = 0.52,
        NoiseOpacity = 0.12, NoiseDensity = 0.34, GlowStrength = 0.22, HorizontalBloom = 0.32,
        VignetteStrength = 0.58, TintOpacity = 0.10, TintRed = 235, TintGreen = 245, TintBlue = 255,
        Gamma = 1.08, Contrast = 0.96, Brightness = 0.02, PhosphorStrength = 0.10, PhosphorScale = 1.0,
        MaskType = 0, FlickerStrength = 0.05, ScreenJitter = 0.25, ReflectionStrength = 0.10, TargetFps = 30
    };
}
