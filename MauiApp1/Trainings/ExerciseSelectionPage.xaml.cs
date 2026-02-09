namespace MauiApp1.Trainings;

public partial class ExerciseSelectionPage : ContentPage
{
    public ExerciseSelectionPage()
    {
        InitializeComponent();
    }

    // Обработчик для кнопки "Таблица Шульте"
    private async void OnShulteTableClicked(object sender, EventArgs e)
    {
        // Переход на страницу тренажера Таблица Шульте
        //await Navigation.PushAsync(new ShulteTablePage()); // Updated to use the correct namespace
    }

    // Обработчик для кнопки "Бегущие слова"
    private async void OnRunningWordsClicked(object sender, EventArgs e)
    {
        // Переход на страницу тренажера Бегущие слова
        //await Navigation.PushAsync(new RunningWordsPage());
    }

}