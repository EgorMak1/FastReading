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
        var page = App.Current!.Handler!.MauiContext!.Services.GetRequiredService<ShulteStatisticsPage>();
        await Navigation.PushAsync(page);
    }

    private async void OnRunningWordsStatisticsClicked(object sender, EventArgs e)
    {
        var page = App.Current!.Handler!.MauiContext!.Services.GetRequiredService<RunningWordsStatisticsPage>();
        await Navigation.PushAsync(page);
    }

    private async void OnFieldOfViewStatisticsClicked(object sender, EventArgs e)
    {
        var page = App.Current!.Handler!.MauiContext!.Services.GetRequiredService<FieldOfViewStatisticsPage>();
        await Navigation.PushAsync(page);
    }
}
