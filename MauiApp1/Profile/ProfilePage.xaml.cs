using MauiApp1.Services;

namespace MauiApp1.Profile
{
    public partial class ProfilePage : ContentPage
    {
        private readonly ProfileService _profileService;
        private CancellationTokenSource? _loadCancellation;
        private bool _isActive;

        public ProfilePage(ProfileService profileService)
        {
            InitializeComponent();
            _profileService = profileService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            _isActive = true;
            _loadCancellation?.Cancel();
            _loadCancellation = new CancellationTokenSource();
            await LoadProfileAsync(_loadCancellation.Token);
        }

        protected override void OnDisappearing()
        {
            _isActive = false;
            _loadCancellation?.Cancel();
            base.OnDisappearing();
        }

        private async Task LoadProfileAsync(CancellationToken cancellationToken)
        {
            try
            {
                var profile = await _profileService.GetProfileAsync(cancellationToken);
                if (!_isActive || cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                if (profile == null)
                {
                    SummaryLabel.Text = "Не удалось загрузить профиль.";
                    return;
                }

                SummaryLabel.Text = "Система оценивает накопленный прогресс, устойчивость выполнения и подбирает следующее направление тренировки.";
                OverallScoreLabel.Text = $"{profile.OverallScore:F1}";
                TodayPointsLabel.Text = $"{profile.TodayPoints:F1}";
                TotalSessionsLabel.Text = profile.TotalSessions.ToString();
                ExercisesTrackedLabel.Text = profile.ExercisesTracked.ToString();
                ReadinessLabel.Text = profile.Readiness;

                InsightsLabel.Text = BuildInsights(profile);
                RecommendationLabel.Text = profile.Recommendation;
                ExerciseProgressContainer.Children.Clear();

                foreach (var exercise in profile.ExerciseProgress.OrderByDescending(x => x.LastPlayedAt))
                {
                    if (!_isActive || cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    ExerciseProgressContainer.Children.Add(CreateExerciseCard(exercise));
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private static string BuildInsights(UserProfileDto profile)
        {
            if (profile.ExerciseProgress.Count == 0)
            {
                return "Пока нет данных для аналитики. Выполните хотя бы одну тренировку.";
            }

            return $"Сильная сторона: {ToDisplayName(profile.StrongestExercise)}. " +
                   $"Наиболее стабильное упражнение: {ToDisplayName(profile.MostStableExercise)}. " +
                   $"Зона роста: {ToDisplayName(profile.WeakestExercise)}. " +
                   $"Требует внимания: {ToDisplayName(profile.NeedsAttentionExercise)}.";
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
                            Text = $"Тренд: {exercise.Trend} | Статус: {exercise.Status}",
                            TextColor = Color.FromArgb("#5C6BC0")
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
