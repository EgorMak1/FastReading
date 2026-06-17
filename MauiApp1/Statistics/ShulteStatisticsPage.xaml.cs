using MauiApp1.Controls;
using MauiApp1.Services;

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
        List<ShulteResultDto> results;

        try
        {
            results = await _statisticsService.GetShulteResultsAsync();
        }
        catch (Exception ex)
        {
            ShowLoadError(ex);
            return;
        }

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
            ChartView.Drawable = null;
            return;
        }

        var last = shulteResults.Last();
        var bestTime = shulteResults.Min(r => r.DurationSeconds);
        var averageTime = shulteResults.Average(r => r.DurationSeconds);
        var bestScore = shulteResults.Max(r => r.Score);
        var averageErrors = shulteResults.Average(r => r.ErrorsCount);

        SummaryLabel.Text = "Статистика учитывает время, ошибки, уровень сложности и итоговые очки.";
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
            $"Очки: {last.Score:F1}\n" +
            $"Уровень: {last.LevelBefore} -> {last.LevelAfter}";

        ChartView.Drawable = new StyledLineChartDrawable(
            shulteResults.Select(r => new LineChartPoint(r.CompletedAt, r.Score)).ToList(),
            Application.Current?.RequestedTheme == AppTheme.Dark,
            "%",
            0,
            100);
        ChartView.Invalidate();
    }

    private void ShowLoadError(Exception exception)
    {
        SummaryLabel.Text = ApiError.FromException(exception, "Не удалось загрузить статистику упражнения.").Message;
        BestResultLabel.Text = "-";
        AverageResultLabel.Text = "-";
        BestScoreLabel.Text = "-";
        CurrentLevelLabel.Text = "-";
        TotalTrainingsLabel.Text = "-";
        AverageErrorsLabel.Text = "-";
        LastSessionLabel.Text = "Статистика временно недоступна.";
        ChartView.Drawable = null;
        ChartView.Invalidate();
    }
}
