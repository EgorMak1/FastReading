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
        var results = await _statisticsService.GetShulteResultsAsync();
        var shulteResults = results.OrderBy(r => r.CompletedAt).ToList();

        if (shulteResults.Count == 0)
        {
            SummaryLabel.Text = "Пока нет данных по таблице Шульте.";
            BestResultLabel.Text = "-";
            AverageResultLabel.Text = "-";
            BestScoreLabel.Text = "-";
            CurrentLevelLabel.Text = "-";
            TotalTrainingsLabel.Text = "0";
            AverageErrorsLabel.Text = "-";
            LastSessionLabel.Text = "Нет завершённых тренировок.";
            ChartView.Drawable = new EmptyChartDrawable("Пока нет данных");
            ChartView.Invalidate();
            return;
        }

        var last = shulteResults.Last();
        var bestTime = shulteResults.Min(r => r.DurationSeconds);
        var averageTime = shulteResults.Average(r => r.DurationSeconds);
        var bestScore = shulteResults.Max(r => r.Score);
        var averageErrors = shulteResults.Average(r => r.ErrorsCount);

        SummaryLabel.Text = "Статистика учитывает время, ошибки, уровень сложности и итоговый score.";
        BestResultLabel.Text = $"{bestTime} сек";
        AverageResultLabel.Text = $"{averageTime:F1} сек";
        BestScoreLabel.Text = $"{bestScore:F1}";
        CurrentLevelLabel.Text = last.LevelAfter.ToString();
        TotalTrainingsLabel.Text = shulteResults.Count.ToString();
        AverageErrorsLabel.Text = $"{averageErrors:F1}";
        LastSessionLabel.Text =
            $"Дата: {last.CompletedAt.ToLocalTime():dd.MM.yyyy HH:mm}\n" +
            $"Время: {last.DurationSeconds} сек\n" +
            $"Ошибки: {last.ErrorsCount}\n" +
            $"Score: {last.Score:F1}\n" +
            $"Уровень: {last.LevelBefore} -> {last.LevelAfter}";

        ChartView.Drawable = new ShulteScoreChartDrawable(shulteResults);
        ChartView.Invalidate();
    }
}

public class ShulteScoreChartDrawable : IDrawable
{
    private readonly List<ShulteResultDto> _results;

    public ShulteScoreChartDrawable(List<ShulteResultDto> results)
    {
        _results = results;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (_results.Count == 0)
        {
            return;
        }

        float paddingLeft = 48f;
        float paddingRight = 18f;
        float paddingTop = 20f;
        float paddingBottom = 36f;
        float width = dirtyRect.Width - paddingLeft - paddingRight;
        float height = dirtyRect.Height - paddingTop - paddingBottom;

        if (width <= 0 || height <= 0)
        {
            return;
        }

        canvas.FillColor = Colors.White;
        canvas.FillRectangle(dirtyRect);

        const float minVal = 0f;
        const float maxVal = 100f;

        canvas.StrokeColor = Color.FromArgb("#D6D6D6");
        canvas.StrokeSize = 1;

        for (int tick = 0; tick <= 4; tick++)
        {
            float y = paddingTop + (height / 4f) * tick;
            canvas.DrawLine(paddingLeft, y, paddingLeft + width, y);
        }

        canvas.StrokeColor = Colors.Gray;
        canvas.DrawLine(paddingLeft, paddingTop, paddingLeft, paddingTop + height);
        canvas.DrawLine(paddingLeft, paddingTop + height, paddingLeft + width, paddingTop + height);

        var points = new List<PointF>();
        for (int i = 0; i < _results.Count; i++)
        {
            float x = paddingLeft + (i / (float)Math.Max(1, _results.Count - 1)) * width;
            float y = paddingTop + height - (float)((_results[i].Score - minVal) / (maxVal - minVal)) * height;
            points.Add(new PointF(x, y));
        }

        canvas.StrokeColor = Color.FromArgb("#5C6BC0");
        canvas.StrokeSize = 3;
        for (int i = 0; i < points.Count - 1; i++)
        {
            canvas.DrawLine(points[i].X, points[i].Y, points[i + 1].X, points[i + 1].Y);
        }

        canvas.FillColor = Color.FromArgb("#5C6BC0");
        foreach (var point in points)
        {
            canvas.FillCircle(point.X, point.Y, 4);
        }

        canvas.FontColor = Colors.Gray;
        canvas.FontSize = 11;
        canvas.DrawString("100", 0, paddingTop - 8, paddingLeft - 8, 20, HorizontalAlignment.Right, VerticalAlignment.Center);
        canvas.DrawString("50", 0, paddingTop + height / 2f - 8, paddingLeft - 8, 20, HorizontalAlignment.Right, VerticalAlignment.Center);
        canvas.DrawString("0", 0, paddingTop + height - 8, paddingLeft - 8, 20, HorizontalAlignment.Right, VerticalAlignment.Center);

        canvas.DrawString("1", paddingLeft - 6, paddingTop + height + 4, 20, 20, HorizontalAlignment.Left, VerticalAlignment.Top);
        canvas.DrawString(_results.Count.ToString(), paddingLeft + width - 16, paddingTop + height + 4, 24, 20, HorizontalAlignment.Right, VerticalAlignment.Top);
    }
}

public class EmptyChartDrawable : IDrawable
{
    private readonly string _message;

    public EmptyChartDrawable(string message)
    {
        _message = message;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.FillColor = Colors.White;
        canvas.FillRectangle(dirtyRect);
        canvas.FontColor = Colors.Gray;
        canvas.FontSize = 16;
        canvas.DrawString(_message, dirtyRect, HorizontalAlignment.Center, VerticalAlignment.Center);
    }
}
