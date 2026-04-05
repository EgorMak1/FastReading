using MauiApp1.Services;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

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
        // Получаем данные с сервера
        var results = await _statisticsService.GetResultsAsync();

        // Фильтруем только результаты Таблицы Шульте
        var shulteResults = results
            .Where(r => r.ExerciseType == "ShulteTable")
            .OrderBy(r => r.CompletedAt)
            .ToList();

        if (shulteResults.Count == 0)
        {
            BestResultLabel.Text = "Нет данных";
            return;
        }

        // Считаем статистику
        var best = shulteResults.Min(r => r.DurationSeconds);
        var average = shulteResults.Average(r => r.DurationSeconds);
        var total = shulteResults.Count;

        BestResultLabel.Text = $" Лучший результат: {best} сек";
        AverageResultLabel.Text = $" Среднее время: {average:F1} сек";
        TotalTrainingsLabel.Text = $" Всего тренировок: {total}";

        // Строим график
        var model = new PlotModel { Title = "Прогресс" };

        // Ось X — дата
        model.Axes.Add(new DateTimeAxis
        {
            Position = AxisPosition.Bottom,
            StringFormat = "dd.MM",
            Title = "Дата",
            IntervalType = DateTimeIntervalType.Days
        });

        // Ось Y — время в секундах
        model.Axes.Add(new LinearAxis
        {
            Position = AxisPosition.Left,
            Title = "Секунды",
            Minimum = 0
        });

        // Линия прогресса
        var series = new LineSeries
        {
            Title = "Время",
            Color = OxyColors.DodgerBlue,
            MarkerType = MarkerType.Circle,
            MarkerSize = 5,
            MarkerFill = OxyColors.DodgerBlue
        };

        foreach (var result in shulteResults)
        {
            series.Points.Add(new DataPoint(
                DateTimeAxis.ToDouble(result.CompletedAt.ToLocalTime()),
                result.DurationSeconds));
        }

        model.Series.Add(series);
        PlotView.Model = model;
    }
}