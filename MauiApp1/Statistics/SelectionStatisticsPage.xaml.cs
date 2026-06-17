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

    private void OnSelectionStatisticsScrollSizeChanged(object sender, EventArgs e)
    {
        SelectionStatisticsContentContainer.MinimumHeightRequest = Math.Max(0, SelectionStatisticsScroll.Height);
    }

    private async void OnShulteStatisticsTapped(object sender, TappedEventArgs e)
    {
        await OpenShulteStatisticsAsync();
    }

    private async void OnRunningWordsStatisticsTapped(object sender, TappedEventArgs e)
    {
        await OpenRunningWordsStatisticsAsync();
    }

    private async void OnFieldOfViewStatisticsTapped(object sender, TappedEventArgs e)
    {
        await OpenFieldOfViewStatisticsAsync();
    }

    private async void OnWordErasingStatisticsTapped(object sender, TappedEventArgs e)
    {
        await OpenWordErasingStatisticsAsync();
    }

    private async Task OpenShulteStatisticsAsync()
    {
        var page = App.Current!.Handler!.MauiContext!.Services.GetRequiredService<ShulteStatisticsPage>();
        await Navigation.PushAsync(page);
    }

    private async Task OpenRunningWordsStatisticsAsync()
    {
        var page = App.Current!.Handler!.MauiContext!.Services.GetRequiredService<RunningWordsStatisticsPage>();
        await Navigation.PushAsync(page);
    }

    private async Task OpenFieldOfViewStatisticsAsync()
    {
        var page = App.Current!.Handler!.MauiContext!.Services.GetRequiredService<FieldOfViewStatisticsPage>();
        await Navigation.PushAsync(page);
    }

    private async Task OpenWordErasingStatisticsAsync()
    {
        var page = App.Current!.Handler!.MauiContext!.Services.GetRequiredService<WordErasingStatisticsPage>();
        await Navigation.PushAsync(page);
    }
}
