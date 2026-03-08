using System;
using System.Windows;
using System.Windows.Media;
using CrtOverlayApp.Models;
using MediaColor = System.Windows.Media.Color;
using MediaPen = System.Windows.Media.Pen;
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
        var jitterX = Math.Sin(_time * 2.6) * _settings.ScreenJitter;
        var jitterY = Math.Cos(_time * 1.7) * (_settings.ScreenJitter * 0.35);

        dc.PushTransform(new TranslateTransform(jitterX, jitterY));
        DrawBrightnessResponse(dc, width, height, master);
        DrawTint(dc, width, height, master);
        DrawHorizontalBloom(dc, width, height, master);
        DrawScreenGlow(dc, width, height, master);
        DrawPhosphorMask(dc, width, height, master);
        DrawScanlines(dc, width, height, master);
        DrawNoise(dc, width, height, master);
        DrawReflection(dc, width, height, master);
        DrawVignette(dc, width, height, master);
        dc.Pop();
    }

    private void DrawBrightnessResponse(DrawingContext dc, double width, double height, double master)
    {
        var brightness = Math.Clamp(_settings.Brightness, -0.35, 0.35);
        if (Math.Abs(brightness) > 0.0001)
        {
            var alpha = (byte)(Math.Min(Math.Abs(brightness) * master * 255.0, 90));
            var color = brightness >= 0 ? MediaColor.FromArgb(alpha, 255, 255, 255) : MediaColor.FromArgb(alpha, 0, 0, 0);
            dc.DrawRectangle(new SolidColorBrush(color), null, new Rect(0, 0, width, height));
        }

        var contrastAmount = Math.Abs(_settings.Contrast - 1.0);
        if (contrastAmount > 0.001)
        {
            var edgeAlpha = (byte)(Math.Min(contrastAmount * 70.0 * master, 85));
            var centerAlpha = (byte)(Math.Min(contrastAmount * 26.0 * master, 40));
            var brush = new RadialGradientBrush
            {
                Center = new WpfPoint(0.5, 0.5),
                GradientOrigin = new WpfPoint(0.5, 0.5),
                RadiusX = 0.82,
                RadiusY = 0.82
            };

            if (_settings.Contrast >= 1.0)
            {
                brush.GradientStops.Add(new GradientStop(MediaColor.FromArgb(centerAlpha, 255, 255, 255), 0.0));
                brush.GradientStops.Add(new GradientStop(MediaColor.FromArgb(0, 255, 255, 255), 0.55));
                brush.GradientStops.Add(new GradientStop(MediaColor.FromArgb(edgeAlpha, 0, 0, 0), 1.0));
            }
            else
            {
                brush.GradientStops.Add(new GradientStop(MediaColor.FromArgb(centerAlpha, 0, 0, 0), 0.0));
                brush.GradientStops.Add(new GradientStop(MediaColor.FromArgb(0, 0, 0, 0), 0.55));
                brush.GradientStops.Add(new GradientStop(MediaColor.FromArgb(edgeAlpha, 255, 255, 255), 1.0));
            }

            dc.DrawRectangle(brush, null, new Rect(0, 0, width, height));
        }

        var gammaAmount = Math.Abs(_settings.Gamma - 1.0);
        if (gammaAmount > 0.001)
        {
            var gammaAlpha = (byte)(Math.Min(gammaAmount * 38.0 * master, 48));
            var brush = new LinearGradientBrush
            {
                StartPoint = new WpfPoint(0.0, 0.0),
                EndPoint = new WpfPoint(0.0, 1.0)
            };

            if (_settings.Gamma > 1.0)
            {
                brush.GradientStops.Add(new GradientStop(MediaColor.FromArgb(gammaAlpha, 0, 0, 0), 0.0));
                brush.GradientStops.Add(new GradientStop(MediaColor.FromArgb(0, 0, 0, 0), 0.45));
                brush.GradientStops.Add(new GradientStop(MediaColor.FromArgb(gammaAlpha, 0, 0, 0), 1.0));
            }
            else
            {
                brush.GradientStops.Add(new GradientStop(MediaColor.FromArgb(gammaAlpha, 255, 255, 255), 0.0));
                brush.GradientStops.Add(new GradientStop(MediaColor.FromArgb(0, 255, 255, 255), 0.45));
                brush.GradientStops.Add(new GradientStop(MediaColor.FromArgb(gammaAlpha, 255, 255, 255), 1.0));
            }

            dc.DrawRectangle(brush, null, new Rect(0, 0, width, height));
        }
    }

    private void DrawTint(DrawingContext dc, double width, double height, double master)
    {
        var alpha = (byte)Math.Clamp(_settings.TintOpacity * master * 255.0, 0.0, 255.0);
        if (alpha <= 0) return;
        dc.DrawRectangle(
            new SolidColorBrush(MediaColor.FromArgb(alpha, _settings.TintRed, _settings.TintGreen, _settings.TintBlue)),
            null,
            new Rect(0, 0, width, height));
    }

    private void DrawHorizontalBloom(DrawingContext dc, double width, double height, double master)
    {
        var amount = _settings.HorizontalBloom * master;
        if (amount <= 0.001) return;

        var lineCount = Math.Max(10, (int)(height / 14));
        var maxAlpha = Math.Max(1, (int)(amount * 46.0));

        for (var i = 0; i < lineCount; i++)
        {
            var y = _random.NextDouble() * height;
            var length = width * (0.35 + _random.NextDouble() * 0.65);
            var startX = (width - length) * _random.NextDouble();
            var endX = startX + length;
            var alpha = (byte)_random.Next(1, maxAlpha + 1);

            var pen = new MediaPen(new SolidColorBrush(MediaColor.FromArgb(alpha, 255, 255, 255)), 2.0 + (_settings.HorizontalBloom * 4.0))
            {
                StartLineCap = PenLineCap.Round,
                EndLineCap = PenLineCap.Round
            };

            dc.DrawLine(pen, new WpfPoint(startX, y), new WpfPoint(endX, y));
        }
    }

    private void DrawScreenGlow(DrawingContext dc, double width, double height, double master)
    {
        var alpha = (byte)Math.Clamp(_settings.GlowStrength * master * 160.0, 0.0, 255.0);
        if (alpha <= 0) return;

        var brush = new RadialGradientBrush
        {
            Center = new WpfPoint(0.5, 0.5),
            GradientOrigin = new WpfPoint(0.5, 0.5),
            RadiusX = 0.70,
            RadiusY = 0.70
        };
        brush.GradientStops.Add(new GradientStop(MediaColor.FromArgb(alpha, _settings.TintRed, _settings.TintGreen, _settings.TintBlue), 0.0));
        brush.GradientStops.Add(new GradientStop(MediaColor.FromArgb((byte)(alpha / 2), _settings.TintRed, _settings.TintGreen, _settings.TintBlue), 0.45));
        brush.GradientStops.Add(new GradientStop(MediaColor.FromArgb(0, 0, 0, 0), 1.0));
        dc.DrawRectangle(brush, null, new Rect(0, 0, width, height));
    }

    private void DrawPhosphorMask(DrawingContext dc, double width, double height, double master)
    {
        var strength = _settings.PhosphorStrength * master;
        if (strength <= 0.001) return;

        var scale = Math.Max(1.0, _settings.PhosphorScale);
        var colWidth = Math.Max(1.0, 2.0 * scale);
        var maxX = (int)Math.Ceiling(width / colWidth);

        for (var xIndex = 0; xIndex <= maxX; xIndex++)
        {
            var x = xIndex * colWidth;
            MediaColor color;
            double bandWidth = colWidth;

            switch (_settings.MaskType)
            {
                case 1:
                    bandWidth = colWidth * 0.75;
                    color = (xIndex % 3) switch
                    {
                        0 => MediaColor.FromArgb((byte)(strength * 90), 255, 110, 110),
                        1 => MediaColor.FromArgb((byte)(strength * 90), 120, 255, 120),
                        _ => MediaColor.FromArgb((byte)(strength * 90), 120, 160, 255)
                    };
                    break;
                case 2:
                    bandWidth = colWidth * 0.55;
                    color = (xIndex % 3) switch
                    {
                        0 => MediaColor.FromArgb((byte)(strength * 115), 255, 85, 85),
                        1 => MediaColor.FromArgb((byte)(strength * 115), 85, 255, 120),
                        _ => MediaColor.FromArgb((byte)(strength * 115), 85, 155, 255)
                    };
                    break;
                default:
                    color = (xIndex % 3) switch
                    {
                        0 => MediaColor.FromArgb((byte)(strength * 78), 255, 120, 120),
                        1 => MediaColor.FromArgb((byte)(strength * 78), 120, 255, 140),
                        _ => MediaColor.FromArgb((byte)(strength * 78), 120, 160, 255)
                    };
                    break;
            }

            dc.DrawRectangle(new SolidColorBrush(color), null, new Rect(x, 0, bandWidth, height));
        }
    }

    private void DrawScanlines(DrawingContext dc, double width, double height, double master)
    {
        var spacing = Math.Max(2, _settings.ScanlineSpacing);
        var opacity = _settings.ScanlineOpacity * master;
        if (opacity <= 0.001) return;

        var softOpacity = Math.Clamp(opacity * _settings.ScanlineSoftness, 0.0, 1.0);
        var hardOpacity = Math.Clamp(opacity * (0.45 + (_settings.ScanlineSoftness * 0.55)), 0.0, 1.0);

        for (var y = 0; y < height; y += spacing)
        {
            dc.DrawRectangle(new SolidColorBrush(MediaColor.FromArgb((byte)(softOpacity * 70.0), 0, 0, 0)), null, new Rect(0, y, width, Math.Min(spacing, 2.0 + (_settings.ScanlineSoftness * 2.0))));
            var centerY = y + Math.Max(1.0, spacing * 0.52);
            dc.DrawRectangle(new SolidColorBrush(MediaColor.FromArgb((byte)(hardOpacity * 120.0), 0, 0, 0)), null, new Rect(0, centerY, width, Math.Max(1.0, spacing * 0.22)));
        }
    }

    private void DrawNoise(DrawingContext dc, double width, double height, double master)
    {
        var density = _settings.NoiseDensity;
        var opacity = _settings.NoiseOpacity * master;
        if (density <= 0.001 || opacity <= 0.001) return;

        var count = Math.Max(10, (int)(density * 320.0));
        var maxAlpha = Math.Max(1, (int)(opacity * 120.0));

        for (var i = 0; i < count; i++)
        {
            var isColorNoise = _random.NextDouble() < 0.22;
            var alpha = (byte)_random.Next(1, maxAlpha + 1);
            byte r = 255, g = 255, b = 255;
            if (isColorNoise)
            {
                r = (byte)_random.Next(120, 256);
                g = (byte)_random.Next(120, 256);
                b = (byte)_random.Next(120, 256);
            }

            var brush = new SolidColorBrush(MediaColor.FromArgb(alpha, r, g, b));
            var x = _random.NextDouble() * width;
            var y = _random.NextDouble() * height;
            var w = 1.0 + (_random.NextDouble() * (12.0 + (_settings.HorizontalBloom * 12.0)));
            var h = _random.NextDouble() < 0.85 ? 1.0 : 2.0;
            dc.DrawRectangle(brush, null, new Rect(x, y, w, h));
        }
    }

    private void DrawReflection(DrawingContext dc, double width, double height, double master)
    {
        var amount = _settings.ReflectionStrength * master;
        if (amount <= 0.001) return;

        var diagonal = new LinearGradientBrush { StartPoint = new WpfPoint(0.0, 0.0), EndPoint = new WpfPoint(1.0, 1.0) };
        diagonal.GradientStops.Add(new GradientStop(MediaColor.FromArgb(0, 255, 255, 255), 0.0));
        diagonal.GradientStops.Add(new GradientStop(MediaColor.FromArgb((byte)(amount * 95.0), 255, 255, 255), 0.16));
        diagonal.GradientStops.Add(new GradientStop(MediaColor.FromArgb(0, 255, 255, 255), 0.30));
        diagonal.GradientStops.Add(new GradientStop(MediaColor.FromArgb(0, 255, 255, 255), 1.0));
        dc.DrawRectangle(diagonal, null, new Rect(0, 0, width, height));

        var topGlass = new LinearGradientBrush { StartPoint = new WpfPoint(0.0, 0.0), EndPoint = new WpfPoint(0.0, 1.0) };
        topGlass.GradientStops.Add(new GradientStop(MediaColor.FromArgb((byte)(amount * 75.0), 255, 255, 255), 0.0));
        topGlass.GradientStops.Add(new GradientStop(MediaColor.FromArgb(0, 255, 255, 255), 0.22));
        dc.DrawRectangle(topGlass, null, new Rect(0, 0, width, height * 0.25));
    }

    private void DrawVignette(DrawingContext dc, double width, double height, double master)
    {
        var alpha = (byte)Math.Clamp(_settings.VignetteStrength * master * 185.0, 0.0, 255.0);
        if (alpha <= 0) return;

        var brush = new RadialGradientBrush
        {
            Center = new WpfPoint(0.5, 0.5),
            GradientOrigin = new WpfPoint(0.5, 0.5),
            RadiusX = 0.80,
            RadiusY = 0.80
        };
        brush.GradientStops.Add(new GradientStop(MediaColor.FromArgb(0, 0, 0, 0), 0.45));
        brush.GradientStops.Add(new GradientStop(MediaColor.FromArgb((byte)(alpha / 2), 0, 0, 0), 0.78));
        brush.GradientStops.Add(new GradientStop(MediaColor.FromArgb(alpha, 0, 0, 0), 1.0));
        dc.DrawRectangle(brush, null, new Rect(0, 0, width, height));
    }
}
