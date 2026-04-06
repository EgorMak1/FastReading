using MauiApp1.Services;

namespace MauiApp1.Trainings;

public partial class ExerciseSelectionPage : ContentPage
{
    private readonly StatisticsService _statisticsService;

    public ExerciseSelectionPage(StatisticsService statisticsService)
    {
        InitializeComponent();
        _statisticsService = statisticsService;
    }

    private async void OnShulteTableClicked(object sender, EventArgs e)
    {
        var page = new ShulteTablePage(_statisticsService);
        await Navigation.PushAsync(page);
    }

    private async void OnRunningWordsClicked(object sender, EventArgs e)
    {
        var page = App.Current!.Handler!.MauiContext!.Services.GetRequiredService<RunningWordsPage>();
        await Navigation.PushAsync(page);
    }
}