using MauiApp1.Auth;
using MauiApp1.Services;
using MauiApp1.Statistics;
using MauiApp1.Trainings;
using Microsoft.Extensions.Logging;



namespace MauiApp1
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            // Создаём билдер приложения MAUI
            var builder = MauiApp.CreateBuilder();

            builder
                // Указываем главный класс приложения
                .UseMauiApp<App>()

                // Подключаем шрифты
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            

            // Регистрация страниц в DI

            // Главная страница — одна на всё приложение
            builder.Services.AddSingleton<MainPage>();

            // Остальные страницы создаются при переходе
            builder.Services.AddTransient<ExerciseSelectionPage>();
            builder.Services.AddTransient<SelectionStatisticsPage>();
            builder.Services.AddTransient<RegisterPage>();


#if DEBUG
            builder.Logging.AddDebug();
#endif

            builder.Services.AddHttpClient();

            builder.Services.AddSingleton<ApiClient>();
            builder.Services.AddSingleton<AuthService>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegisterPage>();


            return builder.Build();

        }
    }
}
