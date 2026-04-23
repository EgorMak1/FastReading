using MauiApp1.Services;

namespace MauiApp1.Profile
{
    public partial class ProfilePage : ContentPage
    {
        private readonly ProfileService _profileService;

        public ProfilePage(ProfileService profileService)
        {
            InitializeComponent();
            _profileService = profileService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadProfileAsync();
        }

        private async Task LoadProfileAsync()
        {
            var profile = await _profileService.GetProfileAsync();
            if (profile == null)
            {
                SummaryLabel.Text = "Не удалось загрузить профиль.";
                return;
            }

            SummaryLabel.Text = "Система оценивает текущий прогресс по упражнениям и подстраивает сложность по накопленным результатам.";
            OverallScoreLabel.Text = $"{profile.OverallScore:F1}";
            TodayPointsLabel.Text = $"{profile.TodayPoints:F1}";
            TotalSessionsLabel.Text = profile.TotalSessions.ToString();
            ExercisesTrackedLabel.Text = profile.ExercisesTracked.ToString();

            InsightsLabel.Text = BuildInsights(profile);
            ExerciseProgressContainer.Children.Clear();

            foreach (var exercise in profile.ExerciseProgress.OrderByDescending(x => x.LastPlayedAt))
            {
                ExerciseProgressContainer.Children.Add(CreateExerciseCard(exercise));
            }
        }

        private static string BuildInsights(UserProfileDto profile)
        {
            if (profile.ExerciseProgress.Count == 0)
            {
                return "Пока нет данных для аналитики. Выполните хотя бы одну тренировку.";
            }

            return $"Сильная сторона: {ToDisplayName(profile.StrongestExercise)}. " +
                   $"Зона роста: {ToDisplayName(profile.WeakestExercise)}.";
        }

        private static View CreateExerciseCard(ExerciseProgressDto exercise)
        {
            return new Border
            {
                Stroke = Color.FromArgb("#D0D0D0"),
                StrokeThickness = 1,
                Padding = 12,
                Content = new VerticalStackLayout
                {
                    Spacing = 6,
                    Children =
                    {
                        new Label
                        {
                            Text = ToDisplayName(exercise.ExerciseType),
                            FontAttributes = FontAttributes.Bold,
                            FontSize = 18
                        },
                        new Label
                        {
                            Text = $"Уровень: {exercise.CurrentLevel} | Последний score: {exercise.LastScore:F1} | Средний score: {exercise.AverageScore:F1}"
                        },
                        new Label
                        {
                            Text = $"Лучший score: {exercise.BestScore:F1} | Сессий: {exercise.SessionsCount}"
                        },
                        new Label
                        {
                            Text = $"Серия успехов: {exercise.SuccessStreak} | Серия неудач: {exercise.FailStreak}"
                        },
                        new Label
                        {
                            Text = $"Последняя тренировка: {exercise.LastPlayedAt.ToLocalTime():dd.MM.yyyy HH:mm}",
                            TextColor = Colors.Gray,
                            FontSize = 13
                        }
                    }
                }
            };
        }

        private static string ToDisplayName(string? exerciseType)
        {
            return exerciseType switch
            {
                "ShulteTable" => "Таблица Шульте",
                "RunningWords" => "Бегущие слова",
                "FieldOfView" => "Поле зрения",
                "WordErasing" => "Затирание слов",
                null or "" => "Нет данных",
                _ => exerciseType
            };
        }
    }
}
