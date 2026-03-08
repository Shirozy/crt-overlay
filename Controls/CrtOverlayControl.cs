using System;
using System.Windows;
using System.Windows.Media;
using MediaPen = System.Windows.Media.Pen;
using CrtOverlayApp.Models;
using MediaColor = System.Windows.Media.Color;
using WpfPoint = System.Windows.Point;

namespace CrtOverlayApp.Controls;

public sealed class CrtOverlayControl : FrameworkElement
{
    private readonly Random _random = new();
    private OverlaySettings _settings = new();
    private double _time;

    public OverlaySettings Settings
    {
        get => _settings;
        set
        {
            _settings = value;
            InvalidateVisual();
        }
    }

    public void AdvanceFrame(double deltaSeconds)
    {
        _time += deltaSeconds;
        InvalidateVisual();
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        var width = ActualWidth;
        var height = ActualHeight;
        if (width <= 0 || height <= 0)
        {
            return;
        }

        var flicker = 1.0 + (Math.Sin(_time * 65.0) * _settings.FlickerStrength);
        var master = Math.Clamp(_settings.MasterOpacity * flicker, 0.0, 1.0);

        DrawTint(dc, width, height, master);
        DrawScreenGlow(dc, width, height, master);
        DrawScanlines(dc, width, height, master);
        DrawNoise(dc, width, height, master);
        DrawRgbShadowMask(dc, width, height, master);
        DrawVignette(dc, width, height, master);
    }


    private void DrawVignette(DrawingContext dc, double width, double height, double master)
    {
        var strength = Math.Clamp(_settings.VignetteStrength, 0, 1);
        if (strength <= 0)
        {
            return;
        }

        var alpha = (byte)(strength * 205 * master);
        var brush = new RadialGradientBrush
        {
            Center = new WpfPoint(0.5, 0.5),
            GradientOrigin = new WpfPoint(0.5, 0.5),
            RadiusX = 0.92,
            RadiusY = 0.92
        };
        brush.GradientStops.Add(new GradientStop(MediaColor.FromArgb(0, 0, 0, 0), 0.42));
        brush.GradientStops.Add(new GradientStop(MediaColor.FromArgb((byte)(alpha * 0.32), 0, 0, 0), 0.72));
        brush.GradientStops.Add(new GradientStop(MediaColor.FromArgb(alpha, 0, 0, 0), 1.0));
        brush.Freeze();

        dc.DrawRectangle(brush, null, new Rect(0, 0, width, height));
    }

    private void DrawTint(DrawingContext dc, double width, double height, double master)
    {
        var alpha = (byte)(Math.Clamp(_settings.TintOpacity, 0, 1) * 140 * master);
        if (alpha <= 0)
        {
            return;
        }

        var brush = new SolidColorBrush(MediaColor.FromArgb(alpha, _settings.TintRed, _settings.TintGreen, _settings.TintBlue));
        brush.Freeze();
        dc.DrawRectangle(brush, null, new Rect(0, 0, width, height));
    }

    private void DrawScreenGlow(DrawingContext dc, double width, double height, double master)
    {
        var strength = Math.Clamp(_settings.GlowStrength, 0, 1);
        if (strength <= 0)
        {
            return;
        }

        var alpha = (byte)(strength * 105 * master);
        var centerGlow = new RadialGradientBrush
        {
            Center = new WpfPoint(0.5, 0.5),
            GradientOrigin = new WpfPoint(0.5, 0.47),
            RadiusX = 0.65,
            RadiusY = 0.58
        };
        centerGlow.GradientStops.Add(new GradientStop(MediaColor.FromArgb(alpha, _settings.TintRed, _settings.TintGreen, _settings.TintBlue), 0.0));
        centerGlow.GradientStops.Add(new GradientStop(MediaColor.FromArgb((byte)(alpha * 0.45), _settings.TintRed, _settings.TintGreen, _settings.TintBlue), 0.45));
        centerGlow.GradientStops.Add(new GradientStop(MediaColor.FromArgb(0, _settings.TintRed, _settings.TintGreen, _settings.TintBlue), 1.0));
        centerGlow.Freeze();
        dc.DrawRectangle(centerGlow, null, new Rect(0, 0, width, height));

        var horizontalAlpha = (byte)(strength * 22 * master);
        if (horizontalAlpha <= 0)
        {
            return;
        }

        var brush = new SolidColorBrush(MediaColor.FromArgb(horizontalAlpha, _settings.TintRed, _settings.TintGreen, _settings.TintBlue));
        brush.Freeze();
        var bandHeight = Math.Max(2.0, _settings.ScanlineSpacing * 0.8);
        for (double y = 0; y < height; y += Math.Max(8.0, _settings.ScanlineSpacing * 6.0))
        {
            dc.DrawRectangle(brush, null, new Rect(0, y, width, bandHeight));
        }
    }

    private void DrawScanlines(DrawingContext dc, double width, double height, double master)
    {
        var spacing = Math.Max(2, _settings.ScanlineSpacing);
        var alpha = (byte)(Math.Clamp(_settings.ScanlineOpacity, 0, 1) * 255 * master);
        if (alpha <= 0)
        {
            return;
        }

        var brush = new SolidColorBrush(MediaColor.FromArgb(alpha, 0, 0, 0));
        brush.Freeze();

        for (int y = 0; y < height; y += spacing)
        {
            dc.DrawRectangle(brush, null, new Rect(0, y, width, 1));
        }
    }

    private void DrawNoise(DrawingContext dc, double width, double height, double master)
    {
        var noiseCount = (int)(width * height * 0.00003 * (0.25 + _settings.NoiseDensity * 3.0));
        var maxAlpha = (byte)(Math.Clamp(_settings.NoiseOpacity, 0, 1) * 255 * master);
        if (noiseCount <= 0 || maxAlpha <= 0)
        {
            return;
        }

        for (int i = 0; i < noiseCount; i++)
        {
            var upperAlphaExclusive = Math.Max(5, (int)maxAlpha);
            var alpha = (byte)_random.Next(4, upperAlphaExclusive);
            var brush = new SolidColorBrush(MediaColor.FromArgb(alpha, 255, 255, 255));
            brush.Freeze();

            var x = _random.NextDouble() * width;
            var y = _random.NextDouble() * height;
            var w = _random.Next(1, 4);
            var h = _random.Next(1, 3);
            dc.DrawRectangle(brush, null, new Rect(x, y, w, h));
        }
    }

    private void DrawRgbShadowMask(DrawingContext dc, double width, double height, double master)
    {
        var alpha = (byte)(18 * master);
        if (alpha <= 0)
        {
            return;
        }

        var red = new SolidColorBrush(MediaColor.FromArgb(alpha, 255, 80, 80));
        var green = new SolidColorBrush(MediaColor.FromArgb(alpha, 80, 255, 120));
        var blue = new SolidColorBrush(MediaColor.FromArgb(alpha, 80, 160, 255));
        red.Freeze();
        green.Freeze();
        blue.Freeze();

        for (int x = 0; x < width; x += 6)
        {
            dc.DrawRectangle(red, null, new Rect(x, 0, 1, height));
            dc.DrawRectangle(green, null, new Rect(x + 2, 0, 1, height));
            dc.DrawRectangle(blue, null, new Rect(x + 4, 0, 1, height));
        }
    }
}
