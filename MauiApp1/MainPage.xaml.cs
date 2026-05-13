using MauiApp1.Controls;
using MauiApp1.Services;
using MauiApp1.Statistics;
using MauiApp1.Trainings;

namespace MauiApp1
{
    public partial class MainPage : ContentPage
    {
        private readonly ProfileService _profileService;
        private readonly StatisticsService _statisticsService;
        private CancellationTokenSource? _loadCancellation;
        private int _activityPeriodDays = 7;
        private IReadOnlyList<DailyActivityDto> _dailyActivity = [];
        private string? _needsAttentionExercise;

        public MainPage(ProfileService profileService, StatisticsService statisticsService)
        {
            InitializeComponent();
            _profileService = profileService;
            _statisticsService = statisticsService;
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
                _needsAttentionExercise = GetRecommendedExercise(profile);
                RecommendationLabel.Text = BuildRecommendationText(_needsAttentionExercise, profile.Recommendation);
                RecommendationActionButton.Text = string.IsNullOrWhiteSpace(_needsAttentionExercise)
                    ? "Выбрать упражнение"
                    : $"Перейти: {ToDisplayName(_needsAttentionExercise)}";

                _dailyActivity = profile.DailyActivity;
                RenderActivityChart();
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
            RecommendationLabel.Text = "Нет данных";
            _needsAttentionExercise = null;
            RecommendationActionButton.Text = "Выбрать упражнение";
            _dailyActivity = [];
            RenderActivityChart();
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

        private async void OnRecommendationClicked(object sender, EventArgs e)
        {
            Page page = _needsAttentionExercise switch
            {
                "ShulteTable" => CreateShulteTableIntro(),
                "RunningWords" => CreateRunningWordsIntro(),
                "FieldOfView" => CreateFieldOfViewIntro(),
                "WordErasing" => CreateWordErasingIntro(),
                _ => App.Current!.Handler!.MauiContext!.Services.GetRequiredService<ExerciseSelectionPage>()
            };

            await Navigation.PushAsync(page);
        }

        private async void OnActivityPeriodTapped(object sender, TappedEventArgs e)
        {
            var selectedPeriod = await DisplayActionSheet("Период", "Отмена", null, "7 дней", "14 дней");

            if (selectedPeriod == "7 дней")
            {
                _activityPeriodDays = 7;
            }
            else if (selectedPeriod == "14 дней")
            {
                _activityPeriodDays = 14;
            }
            else
            {
                return;
            }

            ActivityPeriodLabel.Text = $"{_activityPeriodDays} дней";
            RenderActivityChart();
        }

        private void RenderActivityChart()
        {
            if (ActivityChartGrid == null || ActivityEmptyLabel == null)
            {
                return;
            }

            ActivityChartGrid.Children.Clear();
            ActivityChartGrid.ColumnDefinitions.Clear();
            ActivityChartGrid.RowDefinitions.Clear();

            var points = _dailyActivity
                .TakeLast(_activityPeriodDays)
                .Select(x => new LineChartPoint(
                    x.Date,
                    x.Sessions > 0 ? Math.Clamp(x.Points / x.Sessions, 0, 100) : 0))
                .ToList();
            var hasData = points.Any(x => x.Value > 0);

            ActivityEmptyLabel.IsVisible = !hasData;
            if (!hasData)
            {
                return;
            }

            var isDark = Application.Current?.RequestedTheme == AppTheme.Dark;
            var chart = new GraphicsView
            {
                Drawable = new StyledLineChartDrawable(points, isDark, "%", 0, 100),
                HeightRequest = 132,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };

            ActivityChartGrid.Children.Add(chart);
        }

        private void OnExitClicked(object sender, EventArgs e)
        {
            Application.Current!.Quit();
        }

        private ExerciseIntroPage CreateShulteTableIntro()
        {
            return new ExerciseIntroPage(
                title: "Таблица Шульте",
                subtitle: "Упражнение на внимание, скорость поиска и устойчивость взгляда.",
                instructions:
                [
                    "Нажимайте числа по порядку от 1 до последнего.",
                    "Старайтесь не терять центр обзора и не искать числа хаотично.",
                    "Ошибки замедляют прогресс, поэтому важны и скорость, и точность."
                ],
                difficultyHint: "Сложность растёт за счёт размера сетки, количества чисел, размера шрифта и визуальных отвлекающих элементов.",
                pageFactory: () => new ShulteTablePage(_statisticsService));
        }

        private ExerciseIntroPage CreateRunningWordsIntro()
        {
            return new ExerciseIntroPage(
                title: "Бегущие слова",
                subtitle: "Упражнение на удержание последовательности и быстрое распознавание слова.",
                instructions:
                [
                    "Смотрите на последовательность слов без пауз и не проговаривайте их вслух.",
                    "После показа выберите последнее слово из вариантов.",
                    "Правильные ответы ускоряют показ, ошибки замедляют его."
                ],
                difficultyHint: "Сложность меняется автоматически через интервал показа: шаг 50 мс вверх или вниз после каждого ответа.",
                pageFactory: () => new RunningWordsPage(_statisticsService));
        }

        private ExerciseIntroPage CreateFieldOfViewIntro()
        {
            return new ExerciseIntroPage(
                title: "Поле зрения",
                subtitle: "Упражнение на периферическое восприятие и быструю реакцию на несовпадение.",
                instructions:
                [
                    "Смотрите в центр сетки и не переводите взгляд на края.",
                    "Если одна или несколько крайних букв отличаются, нажмите «Ошибка».",
                    "При смене размера сетки сначала появится пустое поле, затем нажмите «Готов» и продолжайте."
                ],
                difficultyHint: "Сложность растёт за счёт скорости, размера сетки, числа отличий и использования похожих букв.",
                pageFactory: () => new FieldOfViewPage(_statisticsService));
        }

        private ExerciseIntroPage CreateWordErasingIntro()
        {
            return new ExerciseIntroPage(
                title: "Стирание слов",
                subtitle: "Упражнение на чтение с постепенно исчезающим текстом и проверкой понимания.",
                instructions:
                [
                    "Читайте текст, пока слова постепенно скрываются.",
                    "Можно остановиться раньше или нажать «Готово», если закончили чтение.",
                    "После текста ответьте на вопросы по содержанию."
                ],
                difficultyHint: "Сложность определяется скоростью стирания текста. Она меняется по результатам ответов на вопросы.",
                pageFactory: () => new WordErasingPage(_statisticsService, startImmediately: true));
        }

        private static string ToDisplayName(string? exerciseType)
        {
            return exerciseType switch
            {
                "ShulteTable" => "Таблица Шульте",
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

        private static string? GetRecommendedExercise(UserProfileDto profile)
        {
            if (!string.IsNullOrWhiteSpace(profile.NeedsAttentionExercise))
            {
                return profile.NeedsAttentionExercise;
            }

            return string.IsNullOrWhiteSpace(profile.WeakestExercise)
                ? null
                : profile.WeakestExercise;
        }

        private static string BuildRecommendationText(string? exerciseType, string fallbackRecommendation)
        {
            if (!string.IsNullOrWhiteSpace(exerciseType))
            {
                return $"Рекомендуется тренировать упражнение «{ToDisplayName(exerciseType)}»: оно сейчас требует больше всего внимания.";
            }

            return string.IsNullOrWhiteSpace(fallbackRecommendation)
                ? "Нет данных"
                : LocalizeExerciseNames(fallbackRecommendation);
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
