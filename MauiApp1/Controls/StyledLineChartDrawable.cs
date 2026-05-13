using Microsoft.Maui.Graphics;

namespace MauiApp1.Controls;

public sealed record LineChartPoint(DateTime Date, double Value);

public sealed class StyledLineChartDrawable : IDrawable
{
    private readonly IReadOnlyList<LineChartPoint> _points;
    private readonly bool _isDark;
    private readonly string _valueSuffix;
    private readonly double? _minimumValue;
    private readonly double? _maximumValue;

    public StyledLineChartDrawable(
        IReadOnlyList<LineChartPoint> points,
        bool isDark,
        string valueSuffix = "",
        double? minimumValue = null,
        double? maximumValue = null)
    {
        _points = points;
        _isDark = isDark;
        _valueSuffix = valueSuffix;
        _minimumValue = minimumValue;
        _maximumValue = maximumValue;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (_points.Count == 0)
        {
            return;
        }

        var textColor = _isDark ? Color.FromArgb("#D1D5DB") : Color.FromArgb("#6B7280");
        var gridColor = _isDark ? Color.FromArgb("#374151") : Color.FromArgb("#E5E7EB");
        var lineColor = _isDark ? Color.FromArgb("#93C5FD") : Color.FromArgb("#2563EB");
        var plotLeft = 40f;
        var plotTop = 8f;
        var plotRight = dirtyRect.Width - 4f;
        var plotBottom = dirtyRect.Height - 24f;
        var plotWidth = Math.Max(1, plotRight - plotLeft);
        var plotHeight = Math.Max(1, plotBottom - plotTop);

        var minValue = _minimumValue ?? _points.Min(x => x.Value);
        var maxValue = _maximumValue ?? _points.Max(x => x.Value);
        if (Math.Abs(maxValue - minValue) < 0.001)
        {
            maxValue += 1;
            minValue = Math.Max(0, minValue - 1);
        }

        canvas.FontColor = textColor;
        canvas.FontSize = 9;
        canvas.StrokeColor = gridColor;
        canvas.StrokeSize = 1;

        for (var tick = 0; tick < 5; tick++)
        {
            var ratio = tick / 4d;
            var value = maxValue - (maxValue - minValue) * ratio;
            var y = plotTop + (float)(ratio * plotHeight);
            canvas.DrawLine(plotLeft, y, plotRight, y);
            canvas.DrawString(
                FormatAxisValue(value),
                0,
                y - 7,
                plotLeft - 6,
                14,
                HorizontalAlignment.Right,
                VerticalAlignment.Center);
        }

        canvas.StrokeColor = lineColor;
        canvas.StrokeSize = 2;

        var plotted = _points
            .Select((point, index) =>
            {
                var x = _points.Count == 1
                    ? plotLeft + plotWidth / 2
                    : plotLeft + (float)(index / (double)(_points.Count - 1) * plotWidth);
                var normalized = (point.Value - minValue) / (maxValue - minValue);
                var y = plotBottom - (float)(normalized * plotHeight);
                return new PointF(x, y);
            })
            .ToList();

        for (var i = 1; i < plotted.Count; i++)
        {
            canvas.DrawLine(plotted[i - 1].X, plotted[i - 1].Y, plotted[i].X, plotted[i].Y);
        }

        canvas.FillColor = lineColor;
        foreach (var point in plotted)
        {
            canvas.FillCircle(point.X, point.Y, 3.5f);
        }

        canvas.FontSize = _points.Count > 7 ? 8 : 9;
        canvas.FontColor = textColor;
        for (var i = 0; i < _points.Count; i++)
        {
            if (!ShouldShowXAxisLabel(i, _points.Count))
            {
                continue;
            }

            var x = _points.Count == 1
                ? plotLeft + plotWidth / 2
                : plotLeft + (float)(i / (double)(_points.Count - 1) * plotWidth);
            canvas.DrawString(
                _points[i].Date.ToString("dd.MM"),
                x - 18,
                plotBottom + 6,
                36,
                14,
                HorizontalAlignment.Center,
                VerticalAlignment.Center);
        }
    }

    private string FormatAxisValue(double value)
    {
        var rounded = Math.Abs(value) >= 10 ? value.ToString("0") : value.ToString("0.#");
        return $"{rounded}{_valueSuffix}";
    }

    private static bool ShouldShowXAxisLabel(int index, int total)
    {
        if (total <= 7)
        {
            return true;
        }

        return index == 0 || index == total - 1 || index % 2 == 1;
    }
}
