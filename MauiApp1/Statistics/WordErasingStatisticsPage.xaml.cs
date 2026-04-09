using MauiApp1.Services;
using Microsoft.Maui.Graphics;

namespace MauiApp1.Statistics;

public partial class WordErasingStatisticsPage : ContentPage
{
    private readonly StatisticsService _statisticsService;

    public WordErasingStatisticsPage(StatisticsService statisticsService)
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
        var results = await _statisticsService.GetWordErasingResultsAsync();
        var orderedResults = results.OrderBy(r => r.CompletedAt).ToList();

        if (orderedResults.Count == 0)
        {
            CurrentSpeedLabel.Text = "Текущая скорость: 180 WPM";
            LastAttemptLabel.Text = "Нет данных";
            SpeedChangeLabel.Text = string.Empty;
            NextSpeedLabel.Text = string.Empty;
            TotalAttemptsLabel.Text = string.Empty;
            ChartView.Drawable = null;
            return;
        }

        var last = orderedResults.Last();
        CurrentSpeedLabel.Text = $"Текущая скорость: {last.SpeedAfterWpm} WPM";
        TotalAttemptsLabel.Text = $"Всего попыток: {orderedResults.Count}";

        if (last.QuestionsSkipped)
        {
            LastAttemptLabel.Text = $"Последняя попытка: вопросы не показывались, тип завершения: {GetCompletionText(last.CompletionType)}";
        }
        else
        {
            LastAttemptLabel.Text = $"Последняя попытка: {last.CorrectAnswers}/{last.TotalQuestions} ({last.AccuracyPercent:F0}%), " +
                                    $"тип завершения: {GetCompletionText(last.CompletionType)}";
        }

        string speedDeltaText = last.SpeedDelta > 0
            ? $"+{last.SpeedDelta}"
            : last.SpeedDelta.ToString();

        SpeedChangeLabel.Text = $"Изменение скорости: {last.SpeedBeforeWpm} -> {last.SpeedAfterWpm} ({speedDeltaText})";
        NextSpeedLabel.Text = $"Следующее упражнение будет на скорости {last.SpeedAfterWpm} WPM";

        ChartView.Drawable = new WordErasingWpmChartDrawable(
            orderedResults.Select(r => r.SpeedAfterWpm).ToList());
        ChartView.Invalidate();
    }

    private static string GetCompletionText(string completionType)
    {
        return completionType switch
        {
            "Ready" => "Готово",
            "Stop" => "Стоп",
            _ => "Таймер"
        };
    }
}

public class WordErasingWpmChartDrawable : IDrawable
{
    private readonly List<int> _values;

    public WordErasingWpmChartDrawable(List<int> values)
    {
        _values = values;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (_values.Count == 0)
        {
            return;
        }

        float padding = 40f;
        float width = dirtyRect.Width - padding * 2;
        float height = dirtyRect.Height - padding * 2;

        int maxVal = _values.Max();
        int minVal = _values.Min();
        if (maxVal == minVal)
        {
            maxVal = minVal + 1;
        }

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
            float x = padding + (i / (float)Math.Max(_values.Count - 1, 1)) * width;
            float y = padding + height - ((_values[i] - minVal) / (float)(maxVal - minVal)) * height;
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
        canvas.DrawString(maxVal.ToString(), 0, padding, padding - 2, 20, HorizontalAlignment.Right, VerticalAlignment.Center);
        canvas.DrawString(minVal.ToString(), 0, padding + height - 10, padding - 2, 20, HorizontalAlignment.Right, VerticalAlignment.Center);
        canvas.DrawString("1", padding - 10, padding + height + 5, 40, 20, HorizontalAlignment.Center, VerticalAlignment.Top);

        if (_values.Count > 1)
        {
            canvas.DrawString(_values.Count.ToString(), padding + width - 10, padding + height + 5, 40, 20, HorizontalAlignment.Center, VerticalAlignment.Top);
        }
    }
}
