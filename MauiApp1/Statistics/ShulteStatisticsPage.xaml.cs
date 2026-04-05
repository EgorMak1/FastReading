using MauiApp1.Services;
using Microsoft.Maui.Graphics;

namespace MauiApp1.Statistics;

public partial class ShulteStatisticsPage : ContentPage
{
    private readonly StatisticsService _statisticsService;

    public ShulteStatisticsPage(StatisticsService statisticsService)
    {
        InitializeComponent();
        _statisticsService = statisticsService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadStatisticsAsync();
    }

    private async Task LoadStatisticsAsync()
    {
        var results = await _statisticsService.GetResultsAsync();

        var shulteResults = results
            .Where(r => r.ExerciseType == "ShulteTable")
            .OrderBy(r => r.CompletedAt)
            .ToList();

        if (shulteResults.Count == 0)
        {
            BestResultLabel.Text = "Нет данных";
            return;
        }

        var best = shulteResults.Min(r => r.DurationSeconds);
        var average = shulteResults.Average(r => r.DurationSeconds);
        var total = shulteResults.Count;

        BestResultLabel.Text = $"Лучший результат: {best} сек";
        AverageResultLabel.Text = $"Среднее время: {average:F1} сек";
        TotalTrainingsLabel.Text = $"Всего тренировок: {total}";

        var drawable = new LineChartDrawable(
            shulteResults.Select(r => r.DurationSeconds).ToList(),
            shulteResults.Select(r => r.CompletedAt).ToList()
        );

        ChartView.Drawable = drawable;
        ChartView.Invalidate();
    }
}

public class LineChartDrawable : IDrawable
{
    private readonly List<int> _values;
    private readonly List<DateTime> _dates;

    public LineChartDrawable(List<int> values, List<DateTime> dates)
    {
        _values = values;
        _dates = dates;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (_values.Count == 0) return;

        float padding = 40f;
        float width = dirtyRect.Width - padding * 2;
        float height = dirtyRect.Height - padding * 2;

        int maxVal = _values.Max();
        int minVal = _values.Min();
        if (maxVal == minVal) maxVal = minVal + 1;

        // Фон
        canvas.FillColor = Colors.White;
        canvas.FillRectangle(dirtyRect);

        // Оси
        canvas.StrokeColor = Colors.Gray;
        canvas.StrokeSize = 1;
        canvas.DrawLine(padding, padding, padding, padding + height);
        canvas.DrawLine(padding, padding + height, padding + width, padding + height);

        // Линия графика
        canvas.StrokeColor = Color.FromArgb("#2196F3");
        canvas.StrokeSize = 2;

        var points = new List<PointF>();
        for (int i = 0; i < _values.Count; i++)
        {
            float x = padding + (i / (float)(_values.Count - 1 == 0 ? 1 : _values.Count - 1)) * width;
            float y = padding + height - ((_values[i] - minVal) / (float)(maxVal - minVal)) * height;
            points.Add(new PointF(x, y));
        }

        for (int i = 0; i < points.Count - 1; i++)
        {
            canvas.DrawLine(points[i].X, points[i].Y, points[i + 1].X, points[i + 1].Y);
        }

        // Точки
        canvas.FillColor = Color.FromArgb("#2196F3");
        foreach (var point in points)
        {
            canvas.FillCircle(point.X, point.Y, 5);
        }

        // Подписи оси Y
        canvas.FontColor = Colors.Gray;
        canvas.FontSize = 10;
        canvas.DrawString(maxVal.ToString(), 0, padding, padding - 2, 20, HorizontalAlignment.Right, VerticalAlignment.Center);
        canvas.DrawString(minVal.ToString(), 0, padding + height - 10, padding - 2, 20, HorizontalAlignment.Right, VerticalAlignment.Center);

        // Подписи оси X
        if (_dates.Count > 0)
        {
            canvas.DrawString(_dates.First().ToString("dd.MM"), padding - 10, padding + height + 5, 40, 20, HorizontalAlignment.Center, VerticalAlignment.Top);
            if (_dates.Count > 1)
            {
                canvas.DrawString(_dates.Last().ToString("dd.MM"), padding + width - 10, padding + height + 5, 40, 20, HorizontalAlignment.Center, VerticalAlignment.Top);
            }
        }
    }
}