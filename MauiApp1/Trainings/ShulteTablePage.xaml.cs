using MauiApp1.Services;

namespace MauiApp1.Trainings
{
    public partial class ShulteTablePage : ContentPage
    {
        private const double ContentMaxWidth = 720;
        private const double ContentHorizontalPadding = 32;

        private static readonly IReadOnlyList<ShulteDifficultyConfig> DifficultyLevels =
        [
            new(1, 4, 16, 24, 45, false),
            new(2, 5, 25, 20, 60, false),
            new(3, 5, 25, 18, 50, false),
            new(4, 5, 25, 18, 45, true),
            new(5, 6, 36, 16, 80, true)
        ];

        private readonly StatisticsService _statisticsService;
        private readonly List<Button> _buttons = [];

        private ShulteDifficultyConfig _currentConfig = DifficultyLevels[0];
        private int _currentNumber = 1;
        private int _errors;
        private int _recommendedLevel = 1;
        private int _sessionLevelBefore = 1;
        private bool _timerRunning;
        private bool _isInitialized;
        private bool _isFinishing;
        private DateTime _startTime;

        public ShulteTablePage(StatisticsService statisticsService)
        {
            InitializeComponent();
            _statisticsService = statisticsService;
        }

        private void OnShulteScrollSizeChanged(object sender, EventArgs e)
        {
            UpdateTableLayoutSize();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (_isInitialized)
            {
                return;
            }

            _isInitialized = true;
            await InitializeDifficultyAsync();
            StartNewSession();
        }

        protected override void OnDisappearing()
        {
            _timerRunning = false;
            base.OnDisappearing();
        }

        private async Task InitializeDifficultyAsync()
        {
            try
            {
                var results = await _statisticsService.GetShulteResultsAsync();
                var lastResult = results.LastOrDefault();

                if (lastResult != null)
                {
                    _recommendedLevel = ClampLevel(lastResult.LevelAfter);
                }
            }
            catch
            {
                _recommendedLevel = 1;
            }
        }

        private void StartNewSession()
        {
            _sessionLevelBefore = _recommendedLevel;
            _currentConfig = DifficultyLevels[_sessionLevelBefore - 1];
            _currentNumber = 1;
            _errors = 0;
            _isFinishing = false;

            InitializeTable();
            UpdateDifficultyLabels();
        }

        private void InitializeTable()
        {
            _timerRunning = false;
            ShulteTableGrid.RowDefinitions.Clear();
            ShulteTableGrid.ColumnDefinitions.Clear();
            ShulteTableGrid.Children.Clear();
            _buttons.Clear();

            for (int i = 0; i < _currentConfig.GridSize; i++)
            {
                ShulteTableGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
                ShulteTableGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            }

            var numbers = Enumerable
                .Range(1, _currentConfig.NumbersCount)
                .OrderBy(_ => Guid.NewGuid())
                .ToList();

            for (int row = 0; row < _currentConfig.GridSize; row++)
            {
                for (int col = 0; col < _currentConfig.GridSize; col++)
                {
                    int number = numbers[row * _currentConfig.GridSize + col];
                    var button = new Button
                    {
                        Text = number.ToString(),
                        FontSize = _currentConfig.FontSize,
                        BackgroundColor = GetButtonBackgroundColor(number),
                        TextColor = Colors.Black
                    };

                    button.Clicked += OnButtonClicked;
                    _buttons.Add(button);
                    ShulteTableGrid.Add(button, col, row);
                }
            }

            NextNumberLabel.Text = $"Найди: {_currentNumber}";
            TimerLabel.Text = "Время: 00:00";
            _startTime = DateTime.Now;
            _timerRunning = true;
            UpdateTableLayoutSize();

            Dispatcher.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                if (!_timerRunning)
                {
                    return false;
                }

                TimerLabel.Text = $"Время: {DateTime.Now - _startTime:mm\\:ss}";
                return true;
            });
        }

        private void UpdateTableLayoutSize()
        {
            double scrollWidth = Math.Max(0, ShulteScroll.Width);
            if (scrollWidth <= 0)
            {
                return;
            }

            double contentWidth = Math.Min(ContentMaxWidth, scrollWidth);
            ShulteContentContainer.WidthRequest = contentWidth;

            double availableTableWidth = Math.Max(0, contentWidth - ContentHorizontalPadding);
            double tableSize = Math.Min(GetMaxTableSize(_currentConfig.GridSize), availableTableWidth);
            if (tableSize <= 0)
            {
                return;
            }

            ShulteTableGrid.WidthRequest = tableSize;
            ShulteTableGrid.HeightRequest = tableSize;
            FinishButton.WidthRequest = tableSize;
        }

        private static double GetMaxTableSize(int gridSize)
        {
            return gridSize switch
            {
                <= 4 => 400,
                5 => 480,
                _ => 540
            };
        }

        private Color GetButtonBackgroundColor(int number)
        {
            if (!_currentConfig.UseDistractorColors)
            {
                return Color.FromArgb("#D3D3D3");
            }

            var palette = new[]
            {
                Color.FromArgb("#D8E2FF"),
                Color.FromArgb("#E8DEF8"),
                Color.FromArgb("#D7F0E7"),
                Color.FromArgb("#FFE6CC")
            };

            return palette[number % palette.Length];
        }

        private void UpdateDifficultyLabels()
        {
            DifficultyLabel.Text = $"Уровень {_currentConfig.Level}: сетка {_currentConfig.GridSize}x{_currentConfig.GridSize}, цель до {_currentConfig.TargetDurationSeconds} сек.";
            SessionStatsLabel.Text = $"Рекомендованный стартовый уровень: {_recommendedLevel}. Ошибок в текущей попытке: {_errors}.";
        }

        private async void OnButtonClicked(object? sender, EventArgs e)
        {
            if (sender is not Button button || _isFinishing)
            {
                return;
            }

            int clickedNumber = int.Parse(button.Text);

            if (clickedNumber == _currentNumber)
            {
                button.BackgroundColor = Colors.Green;
                _currentNumber++;

                if (_currentNumber > _currentConfig.NumbersCount)
                {
                    await FinishTrainingAsync();
                    return;
                }

                NextNumberLabel.Text = $"Найди: {_currentNumber}";
            }
            else
            {
                button.BackgroundColor = Colors.Red;
                _errors++;
                UpdateDifficultyLabels();
            }

            await Task.Delay(300);

            if (!_isFinishing)
            {
                button.BackgroundColor = GetButtonBackgroundColor(clickedNumber);
            }
        }

        private async Task FinishTrainingAsync(
            int? overrideDurationSeconds = null,
            int? overrideErrorsCount = null,
            double? overrideScore = null,
            int? overrideLevelAfter = null,
            bool stayOnPage = false,
            string? alertTitle = null)
        {
            if (_isFinishing)
            {
                return;
            }

            _isFinishing = true;
            _timerRunning = false;

            int errorsCount = overrideErrorsCount ?? _errors;
            int durationSeconds = overrideDurationSeconds ?? Math.Max(1, (int)Math.Round((DateTime.Now - _startTime).TotalSeconds));
            var timeSpent = TimeSpan.FromSeconds(durationSeconds);
            bool isCompleted = _currentNumber > _currentConfig.NumbersCount;
            double score = overrideScore ?? (isCompleted ? CalculateScore(durationSeconds, errorsCount, _currentConfig) : 0);
            int levelAfter = overrideLevelAfter ?? (isCompleted
                ? CalculateNextLevel(score, errorsCount, durationSeconds, _currentConfig, _sessionLevelBefore)
                : ClampLevel(_sessionLevelBefore - 1));
            _recommendedLevel = levelAfter;

            string? saveError = null;

            try
            {
                await _statisticsService.SaveResultAsync("ShulteTable", durationSeconds);
                await _statisticsService.SaveShulteResultAsync(new ShulteResultRequest
                {
                    GridSize = _currentConfig.GridSize,
                    NumbersCount = _currentConfig.NumbersCount,
                    LevelBefore = _sessionLevelBefore,
                    LevelAfter = levelAfter,
                    DurationSeconds = durationSeconds,
                    ErrorsCount = errorsCount,
                    Score = score
                });
            }
            catch (Exception ex)
            {
                saveError = ApiError.FromException(ex, "Не удалось сохранить результат тренировки.").Message;
            }

            var resultMessage =
                $"Время: {timeSpent:mm\\:ss}\nОшибки: {errorsCount}\nОчки: {score:F0}\nСледующий уровень: {levelAfter}";

            if (!string.IsNullOrWhiteSpace(saveError))
            {
                resultMessage += $"\n\nТренировка завершена, но результат не удалось сохранить: {saveError}";
            }

            await DisplayAlert(
                alertTitle ?? "Тренировка завершена",
                resultMessage,
                "OK");

            if (stayOnPage)
            {
                StartNewSession();
                return;
            }

            await Navigation.PopAsync();
        }

        private static double CalculateScore(int durationSeconds, int errorsCount, ShulteDifficultyConfig config)
        {
            double timeDelta = durationSeconds - config.TargetDurationSeconds;
            double timePenalty = Math.Max(0, timeDelta * 1.8);
            double speedBonus = Math.Max(0, Math.Min(15, (config.TargetDurationSeconds - durationSeconds) * 0.8));
            double errorPenalty = errorsCount * 8;

            return Math.Clamp(100 - timePenalty - errorPenalty + speedBonus, 0, 100);
        }

        private static int CalculateNextLevel(
            double score,
            int errorsCount,
            int durationSeconds,
            ShulteDifficultyConfig config,
            int currentLevel)
        {
            if (durationSeconds > config.TargetDurationSeconds)
            {
                return ClampLevel(currentLevel - 1);
            }

            if (score >= 85 && errorsCount <= 1)
            {
                return ClampLevel(currentLevel + 1);
            }

            if (score < 55 || errorsCount > 3)
            {
                return ClampLevel(currentLevel - 1);
            }

            return ClampLevel(currentLevel);
        }

        private static int ClampLevel(int level)
        {
            return Math.Clamp(level, 1, DifficultyLevels.Count);
        }

        private async void OnFinishTrainingClicked(object sender, EventArgs e)
        {
            await FinishTrainingAsync();
        }

        private sealed record ShulteDifficultyConfig(
            int Level,
            int GridSize,
            int NumbersCount,
            int FontSize,
            int TargetDurationSeconds,
            bool UseDistractorColors);
    }
}
