using MauiApp1.Controls;
using MauiApp1.Services;

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
        var runningWordsResults = results.OrderBy(r => r.CompletedAt).ToList();

        if (runningWordsResults.Count == 0)
        {
            SummaryLabel.Text = "Пока нет данных по упражнению.";
            BestResultLabel.Text = "-";
            AverageResultLabel.Text = "-";
            CurrentSpeedLabel.Text = "-";
            BestSpeedLabel.Text = "-";
            TotalTrainingsLabel.Text = "0";
            AverageSpeedLabel.Text = "-";
            LastSessionLabel.Text = "Нет завершённых тренировок.";
            ChartView.Drawable = null;
            return;
        }

        var last = runningWordsResults.Last();
        var best = runningWordsResults.Max(r => r.AccuracyPercent);
        var average = runningWordsResults.Average(r => r.AccuracyPercent);
        var bestSpeed = runningWordsResults.Min(r => r.FinalSpeedMilliseconds);
        var averageSpeed = runningWordsResults.Average(r => r.FinalSpeedMilliseconds);

        SummaryLabel.Text = "Система учитывает точность запоминания и текущую скорость показа слов.";
        BestResultLabel.Text = $"{best:F1}%";
        AverageResultLabel.Text = $"{average:F1}%";
        CurrentSpeedLabel.Text = $"{last.FinalSpeedMilliseconds} мс";
        BestSpeedLabel.Text = $"{bestSpeed} мс";
        TotalTrainingsLabel.Text = runningWordsResults.Count.ToString();
        AverageSpeedLabel.Text = $"{averageSpeed:F1} мс";
        LastSessionLabel.Text =
            $"Последняя сессия: {last.CompletedAt.ToLocalTime():dd.MM.yyyy HH:mm}\n" +
            $"Точность: {last.AccuracyPercent:F1}%\n" +
            $"Скорость: {last.FinalSpeedMilliseconds} мс";

        ChartView.Drawable = new StyledLineChartDrawable(
            runningWordsResults.Select(r => new LineChartPoint(r.CompletedAt, r.AccuracyPercent)).ToList(),
            Application.Current?.RequestedTheme == AppTheme.Dark,
            "%",
            0,
            100);
        ChartView.Invalidate();
    }
}
