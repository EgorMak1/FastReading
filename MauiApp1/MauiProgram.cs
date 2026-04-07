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
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddTransient<ExerciseSelectionPage>();
            builder.Services.AddTransient<SelectionStatisticsPage>();
            builder.Services.AddTransient<ShulteStatisticsPage>();
            builder.Services.AddTransient<RunningWordsStatisticsPage>();
            builder.Services.AddTransient<FieldOfViewStatisticsPage>();
            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RunningWordsPage>();
            builder.Services.AddTransient<FieldOfViewPage>();

            builder.Services.AddHttpClient();
            builder.Services.AddSingleton<ApiClient>();
            builder.Services.AddSingleton<AuthService>();
            builder.Services.AddSingleton<StatisticsService>();

            return builder.Build();
        }
    }
}
