using MauiApp1.Services;
using Microsoft.Maui.Graphics;

namespace MauiApp1.Statistics;

public partial class RunningWordsStatisticsPage : ContentPage
{
    private readonly StatisticsService _statisticsService;

    public RunningWordsStatisticsPage(StatisticsService statisticsService)
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
        var results = await _statisticsService.GetRunningWordsResultsAsync();

        var runningWordsResults = results
            .OrderBy(r => r.CompletedAt)
            .ToList();

        if (runningWordsResults.Count == 0)
        {
            BestResultLabel.Text = "Нет данных";
            AverageResultLabel.Text = string.Empty;
            TotalTrainingsLabel.Text = string.Empty;
            ChartView.Drawable = null;
            return;
        }

        var best = runningWordsResults.Max(r => r.AccuracyPercent);
        var average = runningWordsResults.Average(r => r.AccuracyPercent);
        var total = runningWordsResults.Count;

        BestResultLabel.Text = $"Лучший результат: {best:F1}%";
        AverageResultLabel.Text = $"Средний результат: {average:F1}%";
        TotalTrainingsLabel.Text = $"Всего прохождений: {total}";

        var drawable = new RunningWordsChartDrawable(
            runningWordsResults.Select(r => r.AccuracyPercent).ToList()
        );

        ChartView.Drawable = drawable;
        ChartView.Invalidate();
    }
}

public class RunningWordsChartDrawable : IDrawable
{
    private readonly List<double> _values;

    public RunningWordsChartDrawable(List<double> values)
    {
        _values = values;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (_values.Count == 0)
            return;

        float padding = 40f;
        float width = dirtyRect.Width - padding * 2;
        float height = dirtyRect.Height - padding * 2;

        double maxVal = 100;
        double minVal = 0;

        canvas.FillColor = Colors.White;
        canvas.FillRectangle(dirtyRect);

        canvas.StrokeColor = Colors.Gray;
        canvas.StrokeSize = 1;
        canvas.DrawLine(padding, padding, padding, padding + height);
        canvas.DrawLine(padding, padding + height, padding + width, padding + height);

        canvas.StrokeColor = Color.FromArgb("#2196F3");
        canvas.StrokeSize = 2;

        var points = new List<PointF>();

        for (int i = 0; i < _values.Count; i++)
        {
            float x = padding + (i / (float)(_values.Count - 1 == 0 ? 1 : _values.Count - 1)) * width;
            float y = padding + height - (float)((_values[i] - minVal) / (maxVal - minVal)) * height;
            points.Add(new PointF(x, y));
        }

        for (int i = 0; i < points.Count - 1; i++)
        {
            canvas.DrawLine(points[i].X, points[i].Y, points[i + 1].X, points[i + 1].Y);
        }

        canvas.FillColor = Color.FromArgb("#2196F3");

        foreach (var point in points)
        {
            canvas.FillCircle(point.X, point.Y, 5);
        }

        canvas.FontColor = Colors.Gray;
        canvas.FontSize = 10;
        canvas.DrawString("100", 0, padding, padding - 2, 20, HorizontalAlignment.Right, VerticalAlignment.Center);
        canvas.DrawString("0", 0, padding + height - 10, padding - 2, 20, HorizontalAlignment.Right, VerticalAlignment.Center);

        canvas.DrawString("1", padding - 10, padding + height + 5, 40, 20, HorizontalAlignment.Center, VerticalAlignment.Top);

        if (_values.Count > 1)
        {
            canvas.DrawString(_values.Count.ToString(), padding + width - 10, padding + height + 5, 40, 20, HorizontalAlignment.Center, VerticalAlignment.Top);
        }
    }
}