using MauiApp1.Services;
using Microsoft.Maui.Controls.Shapes;

namespace MauiApp1.Trainings;

public partial class FieldOfViewPage : ContentPage
{
    private const int SafeRoundsWithoutMismatch = 2;
    private const int MaxConsecutiveRoundsWithoutMismatch = 2;
    private const int AllowedMistakesOnSevenBySeven = 3;
    private const int SevenBySevenSlowdownStepMilliseconds = 150;
    private const int MaximumSevenBySevenRecoveryIntervalMilliseconds = 1400;
    private const int FiveByFiveRecoveryLevel = 2;

    private static readonly IReadOnlyList<FieldOfViewDifficultyConfig> DifficultyLevels =
    [
        new(1, 5, 1800, 0.40, 1, 4, false),
        new(2, 5, 1450, 0.45, 1, 4, false),
        new(3, 7, 1100, 0.50, 2, 3, false),
        new(4, 7, 850, 0.55, 2, 3, true),
        new(5, 7, 650, 0.60, 3, 2, true)
    ];

    private static readonly char[] EasyLetters = "АБВГДЕЖЗИКЛМНОПРСТУФХ".ToCharArray();
    private static readonly char[] HardLetters = "НОСЕРХУКМ".ToCharArray();

    private readonly Label[] _edgeLabels = new Label[4];
    private readonly List<Label> _fillerLabels = [];
    private readonly Random _random = new();
    private readonly StatisticsService _statisticsService;
    private readonly HashSet<int> _currentMismatchIndexes = [];

    private CancellationTokenSource? _exerciseCancellation;
    private TaskCompletionSource<bool>? _gridConfirmationSource;
    private FieldOfViewDifficultyConfig _currentConfig = DifficultyLevels[0];

    private int _gridSize = DifficultyLevels[0].GridSize;
    private int _currentLevel = 1;
    private int _recommendedLevel = 1;
    private int _currentIntervalMilliseconds = DifficultyLevels[0].IntervalMilliseconds;
    private int _roundsCount;
    private int _correctRoundsCount;
    private int _detectedMismatchCount;
    private int _missedMismatchCount;
    private int _falseAlarmCount;
    private int _consecutiveCorrectRounds;
    private int _roundsSinceGridSetup;
    private int _roundsWithoutMismatch;
    private int _mistakesOnSevenBySeven;

    private char _currentBaseLetter;

    private bool _isRunning;
    private bool _awaitingGridConfirmation;
    private bool _currentRoundScored;
    private bool _statisticsSaved;
    private bool _isInitialized;

    public FieldOfViewPage(StatisticsService statisticsService)
    {
        InitializeComponent();
        _statisticsService = statisticsService;
        BuildFieldGrid();
        PrepareBoardForExercise();
        ApplyLevel(_currentLevel);
        UpdateStatsText();
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
        ApplyLevel(_recommendedLevel);
        UpdateDifficultyLabel();
        UpdateStatsText();
    }

    private async Task InitializeDifficultyAsync()
    {
        try
        {
            var results = await _statisticsService.GetFieldOfViewResultsAsync();
            var lastResult = results.LastOrDefault();

            if (lastResult != null)
            {
                _recommendedLevel = ClampLevel(lastResult.FinalLevel);
            }
        }
        catch
        {
            _recommendedLevel = 1;
        }
    }

    private void BuildFieldGrid()
    {
        FieldGrid.RowDefinitions.Clear();
        FieldGrid.ColumnDefinitions.Clear();
        FieldGrid.Children.Clear();
        Array.Clear(_edgeLabels);
        _fillerLabels.Clear();

        for (int i = 0; i < _gridSize; i++)
        {
            FieldGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
            FieldGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        }

        int middle = _gridSize / 2;
        double fillerFontSize = _gridSize >= 7 ? 18 : 24;
        double edgeFontSize = _gridSize >= 7 ? 24 : 30;

        for (int row = 0; row < _gridSize; row++)
        {
            for (int col = 0; col < _gridSize; col++)
            {
                var border = new Border
                {
                    BackgroundColor = Colors.White,
                    Stroke = Colors.LightGray,
                    StrokeThickness = 1,
                    StrokeShape = new RoundRectangle
                    {
                        CornerRadius = 6
                    }
                };

                if (row == middle && col == middle)
                {
                    border.BackgroundColor = Color.FromArgb("#F3F6FA");
                    border.Content = new Label
                    {
                        FontAttributes = FontAttributes.Bold,
                        FontSize = _gridSize >= 7 ? 12 : 14,
                        HorizontalOptions = LayoutOptions.Center,
                        HorizontalTextAlignment = TextAlignment.Center,
                        Text = "Смотри\nв центр",
                        TextColor = Colors.Gray,
                        VerticalOptions = LayoutOptions.Center,
                        VerticalTextAlignment = TextAlignment.Center
                    };
                }
                else if (IsEdgeCenter(row, col, _gridSize))
                {
                    var label = new Label
                    {
                        FontAttributes = FontAttributes.Bold,
                        FontSize = edgeFontSize,
                        HorizontalOptions = LayoutOptions.Center,
                        HorizontalTextAlignment = TextAlignment.Center,
                        Text = "•",
                        VerticalOptions = LayoutOptions.Center,
                        VerticalTextAlignment = TextAlignment.Center
                    };

                    border.Content = label;
                    _edgeLabels[GetEdgeIndex(row, col, _gridSize)] = label;
                }
                else
                {
                    var label = new Label
                    {
                        FontAttributes = FontAttributes.Bold,
                        FontSize = fillerFontSize,
                        HorizontalOptions = LayoutOptions.Center,
                        HorizontalTextAlignment = TextAlignment.Center,
                        Text = string.Empty,
                        TextColor = Colors.Black,
                        VerticalOptions = LayoutOptions.Center,
                        VerticalTextAlignment = TextAlignment.Center
                    };

                    border.Content = label;
                    _fillerLabels.Add(label);
                }

                FieldGrid.Add(border, col, row);
            }
        }
    }

    private static bool IsEdgeCenter(int row, int col, int gridSize)
    {
        int middle = gridSize / 2;
        return (row == 0 && col == middle)
            || (row == middle && col == 0)
            || (row == middle && col == gridSize - 1)
            || (row == gridSize - 1 && col == middle);
    }

    private static int GetEdgeIndex(int row, int col, int gridSize)
    {
        int middle = gridSize / 2;

        if (row == 0 && col == middle)
        {
            return 0;
        }

        if (row == middle && col == gridSize - 1)
        {
            return 1;
        }

        if (row == gridSize - 1 && col == middle)
        {
            return 2;
        }

        return 3;
    }

    private void ApplyLevel(int level)
    {
        int previousGridSize = _gridSize;

        _currentLevel = ClampLevel(level);
        _currentConfig = DifficultyLevels[_currentLevel - 1];
        _gridSize = _currentConfig.GridSize;
        _currentIntervalMilliseconds = _currentConfig.IntervalMilliseconds;

        bool gridChanged = previousGridSize != _gridSize;

        if (gridChanged)
        {
            _roundsSinceGridSetup = 0;

            if (_gridSize >= 7)
            {
                _mistakesOnSevenBySeven = 0;
            }

            if (_gridSize == 5)
            {
                _mistakesOnSevenBySeven = 0;
            }

            BuildFieldGrid();

            if (_isRunning && _roundsCount > 0)
            {
                EnterGridPreviewMode();
            }
            else
            {
                PrepareBoardForExercise();
            }
        }
        else if (!_awaitingGridConfirmation)
        {
            ResetBoard();
        }

        UpdateDifficultyLabel();
    }

    private void UpdateDifficultyLabel()
    {
        string extra = _gridSize >= 7
            ? $" Ошибок на 7x7: {_mistakesOnSevenBySeven}/{AllowedMistakesOnSevenBySeven}."
            : string.Empty;

        DifficultyLabel.Text =
            $"Уровень {_currentLevel}: поле {_gridSize}x{_gridSize}, интервал {_currentIntervalMilliseconds} мс, до {_currentConfig.MaxMismatchCount} отличий.{extra}";
    }

    private void PrepareBoardForExercise()
    {
        PopulateFillerLetters();
        ResetBoard();
    }

    private void PopulateFillerLetters()
    {
        foreach (var label in _fillerLabels)
        {
            label.Text = GetRandomLetter().ToString();
            label.TextColor = Colors.Black;
        }
    }

    private void ClearFillerLetters()
    {
        foreach (var label in _fillerLabels)
        {
            label.Text = string.Empty;
            label.TextColor = Colors.Black;
        }
    }

    private void ResetBoard()
    {
        _currentMismatchIndexes.Clear();

        foreach (var label in _edgeLabels)
        {
            if (label == null)
            {
                continue;
            }

            label.Text = "•";
            label.TextColor = Colors.Black;
        }
    }

    private void EnterGridPreviewMode()
    {
        _awaitingGridConfirmation = true;
        _gridConfirmationSource = new TaskCompletionSource<bool>();
        _currentRoundScored = true;
        _roundsSinceGridSetup = 0;
        _roundsWithoutMismatch = 0;

        ClearFillerLetters();

        foreach (var label in _edgeLabels)
        {
            if (label == null)
            {
                continue;
            }

            label.Text = "•";
            label.TextColor = Color.FromArgb("#5C6BC0");
        }

        ReadyButton.IsVisible = true;
        ReadyButton.IsEnabled = true;
        ErrorButton.IsEnabled = false;
        StatusLabel.Text = $"Новая сетка {_gridSize}x{_gridSize}. Нажмите «Готов».";
    }

    private async Task WaitForGridConfirmationAsync(CancellationToken cancellationToken)
    {
        if (!_awaitingGridConfirmation || _gridConfirmationSource == null)
        {
            return;
        }

        using var registration = cancellationToken.Register(() => _gridConfirmationSource.TrySetCanceled(cancellationToken));
        await _gridConfirmationSource.Task;
    }

    private void ResetSession()
    {
        _roundsCount = 0;
        _correctRoundsCount = 0;
        _detectedMismatchCount = 0;
        _missedMismatchCount = 0;
        _falseAlarmCount = 0;
        _consecutiveCorrectRounds = 0;
        _currentRoundScored = false;
        _statisticsSaved = false;
        _currentBaseLetter = default;
        _awaitingGridConfirmation = false;
        _gridConfirmationSource = null;
        _roundsSinceGridSetup = 0;
        _roundsWithoutMismatch = 0;
        _mistakesOnSevenBySeven = 0;

        ApplyLevel(_recommendedLevel);
        ReadyButton.IsVisible = false;
        ReadyButton.IsEnabled = false;
        PrepareBoardForExercise();
        UpdateStatsText();
    }

    private async Task RunExerciseLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await WaitForGridConfirmationAsync(cancellationToken);
            ShowNextRound();

            try
            {
                await Task.Delay(_currentIntervalMilliseconds, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            FinalizeCurrentRound();
        }
    }

    private void ShowNextRound()
    {
        _roundsCount++;
        _currentRoundScored = false;
        _currentMismatchIndexes.Clear();
        _currentBaseLetter = GetRandomLetter();

        bool allowMismatch = _roundsSinceGridSetup >= SafeRoundsWithoutMismatch;
        bool forceMismatch = allowMismatch && _roundsWithoutMismatch >= MaxConsecutiveRoundsWithoutMismatch;

        if (allowMismatch && (forceMismatch || _random.NextDouble() < _currentConfig.MismatchChance))
        {
            int mismatchCount = _random.Next(1, _currentConfig.MaxMismatchCount + 1);

            while (_currentMismatchIndexes.Count < mismatchCount)
            {
                _currentMismatchIndexes.Add(_random.Next(_edgeLabels.Length));
            }
        }

        _roundsSinceGridSetup++;
        _roundsWithoutMismatch = _currentMismatchIndexes.Count == 0
            ? allowMismatch ? _roundsWithoutMismatch + 1 : 0
            : 0;

        for (int i = 0; i < _edgeLabels.Length; i++)
        {
            char currentLetter = _currentMismatchIndexes.Contains(i)
                ? GetDifferentLetter(_currentBaseLetter)
                : _currentBaseLetter;

            _edgeLabels[i].Text = currentLetter.ToString();
            _edgeLabels[i].TextColor = Colors.Black;
        }

        StatusLabel.Text = _currentMismatchIndexes.Count == 0
            ? "Следите за буквами."
            : "Есть отличие. Нажмите «Ошибка».";

        UpdateStatsText();
    }

    private char GetDifferentLetter(char sourceLetter)
    {
        char newLetter;

        do
        {
            newLetter = GetRandomLetter();
        } while (newLetter == sourceLetter);

        return newLetter;
    }

    private char GetRandomLetter()
    {
        var letters = _currentConfig.UseHardLetters ? HardLetters : EasyLetters;
        return letters[_random.Next(letters.Length)];
    }

    private void FinalizeCurrentRound()
    {
        if (_currentRoundScored)
        {
            return;
        }

        if (_currentMismatchIndexes.Count > 0)
        {
            _missedMismatchCount++;
            ApplyRoundResult(false, $"Пропуск. Отличий: {_currentMismatchIndexes.Count}.", hadMismatch: true);
            return;
        }

        ApplyRoundResult(true, "Совпадение. Ждите следующий раунд.", hadMismatch: false);
    }

    private void ApplyRoundResult(bool isCorrect, string message, bool hadMismatch)
    {
        _currentRoundScored = true;

        if (isCorrect)
        {
            _correctRoundsCount++;

            if (!hadMismatch)
            {
                _consecutiveCorrectRounds = 0;
                _recommendedLevel = _currentLevel;
                StatusLabel.Text = message;
                UpdateDifficultyLabel();
                UpdateStatsText();
                return;
            }

            _consecutiveCorrectRounds++;

            if (_consecutiveCorrectRounds >= _currentConfig.CorrectRoundsToIncreaseLevel && _currentLevel < DifficultyLevels.Count)
            {
                ApplyLevel(_currentLevel + 1);
                _consecutiveCorrectRounds = 0;
                message += $" Уровень {_currentLevel}.";
            }
        }
        else
        {
            _consecutiveCorrectRounds = 0;

            if (_gridSize >= 7)
            {
                _mistakesOnSevenBySeven++;

                if (_mistakesOnSevenBySeven > AllowedMistakesOnSevenBySeven)
                {
                    ApplyLevel(FiveByFiveRecoveryLevel);
                    _recommendedLevel = _currentLevel;
                    message += $" Возврат на 5x5, интервал {_currentIntervalMilliseconds} мс.";
                    StatusLabel.Text = message;
                    UpdateDifficultyLabel();
                    UpdateStatsText();
                    return;
                }

                _currentIntervalMilliseconds = Math.Min(
                    _currentIntervalMilliseconds + SevenBySevenSlowdownStepMilliseconds,
                    MaximumSevenBySevenRecoveryIntervalMilliseconds);

                _recommendedLevel = _currentLevel;
                message += $" Замедление до {_currentIntervalMilliseconds} мс. Ошибок на 7x7: {_mistakesOnSevenBySeven}/{AllowedMistakesOnSevenBySeven}.";
                StatusLabel.Text = message;
                UpdateDifficultyLabel();
                UpdateStatsText();
                return;
            }

            if (_currentLevel > 1)
            {
                ApplyLevel(_currentLevel - 1);
                message += $" Уровень {_currentLevel}.";
            }
        }

        _recommendedLevel = _currentLevel;
        StatusLabel.Text = message;
        UpdateDifficultyLabel();
        UpdateStatsText();
    }

    private void UpdateStatsText()
    {
        StatsLabel.Text =
            $"Уровень: {_currentLevel}, поле: {_gridSize}x{_gridSize}, интервал: {_currentIntervalMilliseconds} мс. " +
            $"Раундов: {_roundsCount}, верных: {_correctRoundsCount}, найдено: {_detectedMismatchCount}, пропущено: {_missedMismatchCount}, ложных: {_falseAlarmCount}.";
    }

    private async void OnStartClicked(object sender, EventArgs e)
    {
        if (_isRunning)
        {
            return;
        }

        ResetSession();

        _exerciseCancellation?.Cancel();
        _exerciseCancellation = new CancellationTokenSource();
        _isRunning = true;

        StartButton.IsEnabled = false;
        StopButton.IsEnabled = true;
        ErrorButton.IsEnabled = true;

        try
        {
            await RunExerciseLoopAsync(_exerciseCancellation.Token);
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            _isRunning = false;
            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;
            ErrorButton.IsEnabled = false;
            ReadyButton.IsEnabled = false;
        }
    }

    private async void OnStopClicked(object sender, EventArgs e)
    {
        await StopAndSaveAsync("Тренировка остановлена.");
    }

    private void OnReadyClicked(object sender, EventArgs e)
    {
        if (!_awaitingGridConfirmation || _gridConfirmationSource == null)
        {
            return;
        }

        _awaitingGridConfirmation = false;
        ReadyButton.IsVisible = false;
        ReadyButton.IsEnabled = false;
        ErrorButton.IsEnabled = _isRunning;
        PrepareBoardForExercise();
        StatusLabel.Text = $"Сетка {_gridSize}x{_gridSize} подтверждена.";
        _gridConfirmationSource.TrySetResult(true);
        _gridConfirmationSource = null;
    }

    private void OnErrorClicked(object sender, EventArgs e)
    {
        if (!_isRunning || _currentRoundScored || _awaitingGridConfirmation)
        {
            return;
        }

        if (_currentMismatchIndexes.Count > 0)
        {
            _detectedMismatchCount++;
            HighlightMismatch();
            ApplyRoundResult(true, $"Верно. Отличий: {_currentMismatchIndexes.Count}.", hadMismatch: true);
            return;
        }

        _falseAlarmCount++;
        ApplyRoundResult(false, "Ложная тревога.", hadMismatch: false);
    }

    private void HighlightMismatch()
    {
        for (int i = 0; i < _edgeLabels.Length; i++)
        {
            _edgeLabels[i].TextColor = _currentMismatchIndexes.Contains(i) ? Colors.Red : Colors.Black;
        }
    }

    private async Task SaveStatisticsAsync()
    {
        if (_statisticsSaved || _roundsCount == 0)
        {
            return;
        }

        var request = new FieldOfViewResultRequest
        {
            GridSize = _gridSize,
            TotalRounds = _roundsCount,
            CorrectRounds = _correctRoundsCount,
            DetectedMismatchCount = _detectedMismatchCount,
            MissedMismatchCount = _missedMismatchCount,
            FalseAlarmCount = _falseAlarmCount,
            AccuracyPercent = (double)_correctRoundsCount / _roundsCount * 100,
            FinalLevel = _currentLevel,
            FinalIntervalMilliseconds = _currentIntervalMilliseconds
        };

        _statisticsSaved = await _statisticsService.SaveFieldOfViewResultAsync(request);
    }

    private async Task StopAndSaveAsync(string message)
    {
        _exerciseCancellation?.Cancel();
        _gridConfirmationSource?.TrySetCanceled();
        _awaitingGridConfirmation = false;
        _isRunning = false;
        ReadyButton.IsVisible = false;
        ReadyButton.IsEnabled = false;
        StatusLabel.Text = message;
        ResetBoard();
        await SaveStatisticsAsync();
    }

    protected override async void OnDisappearing()
    {
        await StopAndSaveAsync("Тренировка остановлена.");
        base.OnDisappearing();
    }

    private static int ClampLevel(int level)
    {
        return Math.Clamp(level, 1, DifficultyLevels.Count);
    }

    private sealed record FieldOfViewDifficultyConfig(
        int Level,
        int GridSize,
        int IntervalMilliseconds,
        double MismatchChance,
        int MaxMismatchCount,
        int CorrectRoundsToIncreaseLevel,
        bool UseHardLetters);
}
