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

            // Регистрируем HttpClient для работы с FastReading.Server (Production VPS)
            builder.Services.AddHttpClient("FastReadingApi", client =>
            {
                client.BaseAddress = new Uri("http://158.160.172.156:5242/");
            });

            // Регистрируем MainPage в контейнере зависимостей
            builder.Services.AddTransient<MainPage>();

#if DEBUG
            // Логирование в режиме Debug (только при локальной отладке)
            builder.Logging.AddDebug();
#endif

            // Сборка приложения
            return builder.Build();
        }
    }
}
