using MauiApp1.Controls;
using MauiApp1.Services;

namespace MauiApp1.Statistics;

public partial class FieldOfViewStatisticsPage : ContentPage
{
    private readonly StatisticsService _statisticsService;

    public FieldOfViewStatisticsPage(StatisticsService statisticsService)
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
        var results = await _statisticsService.GetFieldOfViewResultsAsync();
        var fieldOfViewResults = results.OrderBy(r => r.CompletedAt).ToList();

        if (fieldOfViewResults.Count == 0)
        {
            SummaryLabel.Text = "Пока нет данных по упражнению.";
            BestResultLabel.Text = "-";
            AverageResultLabel.Text = "-";
            CurrentLevelLabel.Text = "-";
            BestIntervalLabel.Text = "-";
            TotalTrainingsLabel.Text = "0";
            AverageLevelLabel.Text = "-";
            LastSessionLabel.Text = "Нет завершённых тренировок.";
            ChartView.Drawable = null;
            return;
        }

        var last = fieldOfViewResults.Last();
        var best = fieldOfViewResults.Max(r => r.AccuracyPercent);
        var average = fieldOfViewResults.Average(r => r.AccuracyPercent);
        var bestInterval = fieldOfViewResults.Min(r => r.FinalIntervalMilliseconds);
        var averageLevel = fieldOfViewResults.Average(r => r.FinalLevel);

        SummaryLabel.Text = "Статистика показывает, насколько точно пользователь замечает отличия и удерживает сложность при росте скорости и размера поля.";
        BestResultLabel.Text = $"{best:F1}%";
        AverageResultLabel.Text = $"{average:F1}%";
        CurrentLevelLabel.Text = last.FinalLevel.ToString();
        BestIntervalLabel.Text = $"{bestInterval} мс";
        TotalTrainingsLabel.Text = fieldOfViewResults.Count.ToString();
        AverageLevelLabel.Text = $"{averageLevel:F1}";
        LastSessionLabel.Text =
            $"Последняя сессия: {last.CompletedAt.ToLocalTime():dd.MM.yyyy HH:mm}\n" +
            $"Точность: {last.AccuracyPercent:F1}%\n" +
            $"Уровень: {last.FinalLevel}\n" +
            $"Поле: {last.GridSize}x{last.GridSize}\n" +
            $"Интервал: {last.FinalIntervalMilliseconds} мс\n" +
            $"Найдено отличий: {last.DetectedMismatchCount}, пропущено: {last.MissedMismatchCount}, ложных тревог: {last.FalseAlarmCount}";

        ChartView.Drawable = new StyledLineChartDrawable(
            fieldOfViewResults.Select(r => new LineChartPoint(r.CompletedAt, r.AccuracyPercent)).ToList(),
            Application.Current?.RequestedTheme == AppTheme.Dark,
            "%",
            0,
            100);
        ChartView.Invalidate();
    }
}
