using MauiApp1.Controls;
using MauiApp1.Services;

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
        List<WordErasingResultDto> results;

        try
        {
            results = await _statisticsService.GetWordErasingResultsAsync();
        }
        catch (Exception ex)
        {
            ShowLoadError(ex);
            return;
        }

        var orderedResults = results.OrderBy(r => r.CompletedAt).ToList();

        if (orderedResults.Count == 0)
        {
            CurrentSpeedLabel.Text = "Текущая скорость: 180 WPM";
            CurrentBandLabel.Text = "Диапазон сложности: уровень 2 (устойчивый темп)";
            LastAttemptLabel.Text = "Попыток пока нет";
            SpeedChangeLabel.Text = string.Empty;
            NextSpeedLabel.Text = string.Empty;
            TotalAttemptsLabel.Text = string.Empty;
            ChartView.Drawable = null;
            return;
        }

        var last = orderedResults.Last();
        CurrentSpeedLabel.Text = $"Текущая скорость: {last.SpeedAfterWpm} WPM";
        CurrentBandLabel.Text = $"Диапазон сложности: {GetSpeedBandText(last.SpeedAfterWpm)}";
        TotalAttemptsLabel.Text = $"Всего попыток: {orderedResults.Count}";

        if (last.QuestionsSkipped)
        {
            LastAttemptLabel.Text = $"Последняя попытка: вопросы пропущены, тип завершения: {GetCompletionText(last.CompletionType)}";
        }
        else
        {
            LastAttemptLabel.Text = $"Последняя попытка: {last.CorrectAnswers}/{last.TotalQuestions} ({last.AccuracyPercent:F0}%), тип завершения: {GetCompletionText(last.CompletionType)}";
        }

        string speedDeltaText = last.SpeedDelta > 0 ? $"+{last.SpeedDelta}" : last.SpeedDelta.ToString();
        SpeedChangeLabel.Text = $"Изменение скорости: {last.SpeedBeforeWpm} -> {last.SpeedAfterWpm} ({speedDeltaText})";
        NextSpeedLabel.Text = $"Следующее упражнение начнётся со скорости {last.SpeedAfterWpm} WPM";

        var min = Math.Max(0, orderedResults.Min(r => r.SpeedAfterWpm) - 20);
        var max = orderedResults.Max(r => r.SpeedAfterWpm) + 20;
        ChartView.Drawable = new StyledLineChartDrawable(
            orderedResults.Select(r => new LineChartPoint(r.CompletedAt, r.SpeedAfterWpm)).ToList(),
            Application.Current?.RequestedTheme == AppTheme.Dark,
            string.Empty,
            min,
            max);
        ChartView.Invalidate();
    }

    private void ShowLoadError(Exception exception)
    {
        CurrentSpeedLabel.Text = ApiError.FromException(exception, "Не удалось загрузить статистику упражнения.").Message;
        CurrentBandLabel.Text = string.Empty;
        LastAttemptLabel.Text = "Статистика временно недоступна.";
        SpeedChangeLabel.Text = string.Empty;
        NextSpeedLabel.Text = string.Empty;
        TotalAttemptsLabel.Text = string.Empty;
        ChartView.Drawable = null;
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

    private static string GetSpeedBandText(int wpm)
    {
        return wpm switch
        {
            <= 160 => "уровень 1 (базовый темп)",
            <= 220 => "уровень 2 (устойчивый темп)",
            <= 280 => "уровень 3 (ускоренное чтение)",
            <= 340 => "уровень 4 (высокий темп)",
            _ => "уровень 5 (максимальный темп)"
        };
    }
}
