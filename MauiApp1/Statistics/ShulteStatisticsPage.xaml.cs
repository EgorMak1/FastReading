using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using Axis = LiveChartsCore.SkiaSharpView.Axis;
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

        var values = shulteResults
            .Select(r => new DateTimePoint(r.CompletedAt.ToLocalTime(), r.DurationSeconds))
            .ToArray();
        var cartesianChart = (LiveChartsCore.SkiaSharpView.Maui.CartesianChart)Chart;

        cartesianChart.Series = new ISeries[]
        {
        new LineSeries<DateTimePoint>
        {
            Values = values,
            Name = "Время (сек)"
        }
        };

        cartesianChart.XAxes = new[]
        {
        new Axis
        {
            Labeler = value => new DateTime((long)value).ToString("dd.MM"),
            Name = "Дата"
        }
    };

        cartesianChart.YAxes = new[]
        {
        new Axis { Name = "Секунды", MinLimit = 0 }
    };
    }
}
    