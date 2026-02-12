using MauiApp1.Trainings;
using MauiApp1.Statistics;
using System.Net.Http;

using MauiApp1.Auth;

namespace MauiApp1
{
    public partial class MainPage : ContentPage
    {
        // HttpClientFactory для получения настроенного HttpClient
        private readonly IHttpClientFactory _httpClientFactory;

        public MainPage(IHttpClientFactory httpClientFactory)
        {
            InitializeComponent();

            // Сохраняем фабрику клиентов, зарегистрированную в MauiProgram
            _httpClientFactory = httpClientFactory;
        }

        private async void OnStartTrainingClicked(object sender, EventArgs e)
        {
            // Переход на страницу выбора упражнения
            await Navigation.PushAsync(new ExerciseSelectionPage());
        }

        private async void OnViewStatisticsClicked(object sender, EventArgs e)
        {
            // Переход на страницу статистики
            await Navigation.PushAsync(new SelectionStatisticsPage());
        }

        // Обработчик для кнопки "Выйти"
        private void OnExitClicked(object sender, EventArgs e)
        {
            // Завершение приложения
            System.Environment.Exit(0);
        }

        private async void OnAuthButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MauiApp1.Auth.RegisterPage());
        }


    }
}
