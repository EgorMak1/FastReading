using MauiApp1.Services;

namespace MauiApp1.Statistics;

public partial class SelectionStatisticsPage : ContentPage
{
    private readonly StatisticsService _statisticsService;

    public SelectionStatisticsPage(StatisticsService statisticsService)
    {
        InitializeComponent();
        _statisticsService = statisticsService;
    }

    private async void OnShulteStatisticsClicked(object sender, EventArgs e)
    {
        var page = new ShulteStatisticsPage(_statisticsService);
        await Navigation.PushAsync(page);
    }
}