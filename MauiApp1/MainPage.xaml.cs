using MauiApp1.Profile;
using MauiApp1.Services;
using MauiApp1.Statistics;
using MauiApp1.Trainings;

namespace MauiApp1
{
    public partial class MainPage : ContentPage
    {
        private readonly ProfileService _profileService;
        private CancellationTokenSource? _loadCancellation;

        public MainPage(ProfileService profileService)
        {
            InitializeComponent();
            _profileService = profileService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            _loadCancellation?.Cancel();
            _loadCancellation = new CancellationTokenSource();
            await LoadDashboardAsync(_loadCancellation.Token);
        }

        protected override void OnDisappearing()
        {
            _loadCancellation?.Cancel();
            base.OnDisappearing();
        }

        private async Task LoadDashboardAsync(CancellationToken cancellationToken)
        {
            DashboardStatusLabel.Text = "Загрузка...";

            try
            {
                var profile = await _profileService.GetProfileAsync(cancellationToken);
                if (profile == null || cancellationToken.IsCancellationRequested)
                {
                    SetEmptyDashboard();
                    return;
                }

                GreetingLabel.Text = $"Привет, {GetProfileName(profile)}";

                DashboardStatusLabel.Text = string.IsNullOrWhiteSpace(profile.Readiness)
                    ? "Данные профиля загружены."
                    : profile.Readiness;

                TodayPointsLabel.Text = profile.TodayPoints > 0 ? $"{profile.TodayPoints:F1}" : "—";
                TotalSessionsLabel.Text = profile.TotalSessions > 0 ? profile.TotalSessions.ToString() : "—";
                RecommendationLabel.Text = string.IsNullOrWhiteSpace(profile.Recommendation)
                    ? "Нет данных"
                    : LocalizeExerciseNames(profile.Recommendation);

                AccuracyLabel.Text = "—";
            }
            catch (OperationCanceledException)
            {
            }
            catch
            {
                SetEmptyDashboard();
            }
        }

        private void SetEmptyDashboard()
        {
            GreetingLabel.Text = "Привет";
            DashboardStatusLabel.Text = "Нет данных";
            TodayPointsLabel.Text = "—";
            TotalSessionsLabel.Text = "—";
            AccuracyLabel.Text = "—";
            RecommendationLabel.Text = "Нет данных";
        }

        private async void OnStartTrainingClicked(object sender, EventArgs e)
        {
            var page = App.Current!.Handler!.MauiContext!.Services.GetRequiredService<ExerciseSelectionPage>();
            await Navigation.PushAsync(page);
        }

        private async void OnViewStatisticsClicked(object sender, EventArgs e)
        {
            var page = App.Current!.Handler!.MauiContext!.Services.GetRequiredService<SelectionStatisticsPage>();
            await Navigation.PushAsync(page);
        }

        private async void OnViewProfileClicked(object sender, EventArgs e)
        {
            var page = App.Current!.Handler!.MauiContext!.Services.GetRequiredService<ProfilePage>();
            await Navigation.PushAsync(page);
        }

        private void OnExitClicked(object sender, EventArgs e)
        {
            Application.Current!.Quit();
        }

        private static string ToDisplayName(string? exerciseType)
        {
            return exerciseType switch
            {
                "ShulteTable" => "Шульте",
                "RunningWords" => "Бегущие слова",
                "FieldOfView" => "Поле зрения",
                "WordErasing" => "Стирание слов",
                null or "" => "Нет данных",
                _ => exerciseType
            };
        }

        private static string LocalizeExerciseNames(string text)
        {
            return text
                .Replace("ShulteTable", "Таблица Шульте")
                .Replace("RunningWords", "Бегущие слова")
                .Replace("FieldOfView", "Поле зрения")
                .Replace("WordErasing", "Стирание слов");
        }

        private static string GetProfileName(UserProfileDto profile)
        {
            if (!string.IsNullOrWhiteSpace(profile.DisplayName))
            {
                return profile.DisplayName;
            }

            if (!string.IsNullOrWhiteSpace(profile.Username))
            {
                return profile.Username;
            }

            return "читатель";
        }
    }
}
