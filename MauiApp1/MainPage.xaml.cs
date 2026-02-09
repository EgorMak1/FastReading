using MauiApp1.Trainings;
using MauiApp1.Statistics;

namespace MauiApp1;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnStartTrainingClicked(object sender, EventArgs e)
    {
        // Логика для начала тренировки
        await Navigation.PushAsync(new ExerciseSelectionPage());
    }
    private async void OnViewStatisticsClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SelectionStatisticsPage());
    }

    // Обработчик для кнопки "Выйти"
    private void OnExitClicked(object sender, EventArgs e)
    {
        // Логика для выхода из приложения
        System.Environment.Exit(0);
    }

    private async void OnAuthButtonClicked(object sender, EventArgs e)
    {

    }

}
