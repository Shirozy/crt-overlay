using System;
using System.Collections.Generic;

namespace CrtOverlayApp.Models;

public static class OverlayPresets
{
    public static IReadOnlyList<PresetOption> All { get; } = new List<PresetOption>
    {
        new() { Name = "Green Terminal", DisplayName = "Green Terminal" },
        new() { Name = "Blue Monitor", DisplayName = "Blue Monitor" },
        new() { Name = "Amber Terminal", DisplayName = "Amber Terminal" },
        new() { Name = "Sony PVM", DisplayName = "Sony PVM" },
        new() { Name = "VHS", DisplayName = "VHS" },
        new() { Name = "Arcade", DisplayName = "Arcade" },
        new() { Name = "Broadcast", DisplayName = "Broadcast" },
        new() { Name = "Monochrome Green", DisplayName = "Monochrome Green" },
        new() { Name = "Monochrome Amber", DisplayName = "Monochrome Amber" },
        new() { Name = "Subtle CRT", DisplayName = "Subtle CRT" },
    };

    public static OverlaySettings CreateByName(string name) => name switch
    {
        "Green Terminal" => GreenTerminal(),
        "Blue Monitor" => BlueMonitor(),
        "Amber Terminal" => AmberTerminal(),
        "Sony PVM" => SonyPvm(),
        "VHS" => Vhs(),
        "Arcade" => Arcade(),
        "Broadcast" => Broadcast(),
        "Monochrome Green" => MonochromeGreen(),
        "Monochrome Amber" => MonochromeAmber(),
        "Subtle CRT" => SubtleCrt(),
        _ => SonyPvm()
    };

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

    public static OverlaySettings Arcade() => new()
    {
        MasterOpacity = 0.66, ScanlineSpacing = 3, ScanlineOpacity = 0.19, ScanlineSoftness = 0.72,
        NoiseOpacity = 0.07, NoiseDensity = 0.17, GlowStrength = 0.24, HorizontalBloom = 0.28,
        VignetteStrength = 0.42, TintOpacity = 0.12, TintRed = 255, TintGreen = 225, TintBlue = 215,
        Gamma = 1.04, Contrast = 1.18, Brightness = 0.01, PhosphorStrength = 0.22, PhosphorScale = 1.25,
        MaskType = 2, FlickerStrength = 0.028, ScreenJitter = 0.09, ReflectionStrength = 0.06, TargetFps = 60
    };

    public static OverlaySettings Broadcast() => new()
    {
        MasterOpacity = 0.50, ScanlineSpacing = 2, ScanlineOpacity = 0.12, ScanlineSoftness = 0.70,
        NoiseOpacity = 0.03, NoiseDensity = 0.10, GlowStrength = 0.08, HorizontalBloom = 0.08,
        VignetteStrength = 0.25, TintOpacity = 0.05, TintRed = 245, TintGreen = 248, TintBlue = 255,
        Gamma = 0.97, Contrast = 1.10, Brightness = 0.0, PhosphorStrength = 0.16, PhosphorScale = 1.0,
        MaskType = 2, FlickerStrength = 0.01, ScreenJitter = 0.03, ReflectionStrength = 0.02, TargetFps = 60
    };

    public static OverlaySettings MonochromeGreen() => new()
    {
        MasterOpacity = 0.68, ScanlineSpacing = 3, ScanlineOpacity = 0.18, ScanlineSoftness = 0.64,
        NoiseOpacity = 0.04, NoiseDensity = 0.14, GlowStrength = 0.20, HorizontalBloom = 0.18,
        VignetteStrength = 0.44, TintOpacity = 0.22, TintRed = 85, TintGreen = 255, TintBlue = 110,
        Gamma = 1.08, Contrast = 1.08, Brightness = 0.03, PhosphorStrength = 0.08, PhosphorScale = 1.0,
        MaskType = 0, FlickerStrength = 0.02, ScreenJitter = 0.07, ReflectionStrength = 0.04, TargetFps = 30
    };

    public static OverlaySettings MonochromeAmber() => new()
    {
        MasterOpacity = 0.67, ScanlineSpacing = 3, ScanlineOpacity = 0.17, ScanlineSoftness = 0.64,
        NoiseOpacity = 0.04, NoiseDensity = 0.14, GlowStrength = 0.21, HorizontalBloom = 0.20,
        VignetteStrength = 0.44, TintOpacity = 0.22, TintRed = 255, TintGreen = 176, TintBlue = 60,
        Gamma = 1.08, Contrast = 1.08, Brightness = 0.03, PhosphorStrength = 0.08, PhosphorScale = 1.0,
        MaskType = 0, FlickerStrength = 0.02, ScreenJitter = 0.07, ReflectionStrength = 0.04, TargetFps = 30
    };

    public static OverlaySettings SubtleCrt() => new()
    {
        MasterOpacity = 0.38, ScanlineSpacing = 4, ScanlineOpacity = 0.07, ScanlineSoftness = 0.52,
        NoiseOpacity = 0.02, NoiseDensity = 0.08, GlowStrength = 0.06, HorizontalBloom = 0.05,
        VignetteStrength = 0.18, TintOpacity = 0.04, TintRed = 235, TintGreen = 245, TintBlue = 255,
        Gamma = 1.0, Contrast = 1.02, Brightness = 0.0, PhosphorStrength = 0.05, PhosphorScale = 1.0,
        MaskType = 0, FlickerStrength = 0.008, ScreenJitter = 0.01, ReflectionStrength = 0.01, TargetFps = 24
    };
}
