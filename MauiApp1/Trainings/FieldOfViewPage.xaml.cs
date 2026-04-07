using MauiApp1.Services;
using Microsoft.Maui.Controls.Shapes;

namespace MauiApp1.Trainings;

public partial class FieldOfViewPage : ContentPage
{
    private const int GridSize = 5;
    private const int CorrectRoundsToIncreaseLevel = 4;

    private readonly Label[] _edgeLabels = new Label[4];
    private readonly Random _random = new();
    private readonly int[] _speedLevels = { 1800, 1500, 1200, 900, 700 };
    private readonly char[] _letters = "АБВГДЕЖЗИКЛМНОПРСТУФХ".ToCharArray();
    private readonly StatisticsService _statisticsService;

    private CancellationTokenSource? _exerciseCancellation;

    private int _currentLevel = 1;
    private int _currentIntervalMilliseconds = 1800;
    private int _roundsCount;
    private int _correctRoundsCount;
    private int _detectedMismatchCount;
    private int _missedMismatchCount;
    private int _falseAlarmCount;
    private int _consecutiveCorrectRounds;

    private bool _isRunning;
    private bool _currentRoundHasMismatch;
    private bool _currentRoundScored;
    private bool _statisticsSaved;

    public FieldOfViewPage(StatisticsService statisticsService)
    {
        InitializeComponent();
        _statisticsService = statisticsService;
        BuildFieldGrid();
        ResetBoard();
        UpdateStatsText();
    }

    private void BuildFieldGrid()
    {
        FieldGrid.RowDefinitions.Clear();
        FieldGrid.ColumnDefinitions.Clear();
        FieldGrid.Children.Clear();

        for (int i = 0; i < GridSize; i++)
        {
            FieldGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
            FieldGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        }

        for (int row = 0; row < GridSize; row++)
        {
            for (int col = 0; col < GridSize; col++)
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

                if (row == GridSize / 2 && col == GridSize / 2)
                {
                    border.BackgroundColor = Color.FromArgb("#F3F6FA");
                    border.Content = new Label
                    {
                        FontAttributes = FontAttributes.Bold,
                        FontSize = 14,
                        HorizontalOptions = LayoutOptions.Center,
                        HorizontalTextAlignment = TextAlignment.Center,
                        Text = "Смотри\nв центр",
                        TextColor = Colors.Gray,
                        VerticalOptions = LayoutOptions.Center,
                        VerticalTextAlignment = TextAlignment.Center
                    };
                }
                else if (IsEdgeCenter(row, col))
                {
                    var label = new Label
                    {
                        FontAttributes = FontAttributes.Bold,
                        FontSize = 30,
                        HorizontalOptions = LayoutOptions.Center,
                        HorizontalTextAlignment = TextAlignment.Center,
                        Text = "•",
                        VerticalOptions = LayoutOptions.Center,
                        VerticalTextAlignment = TextAlignment.Center
                    };

                    border.Content = label;
                    _edgeLabels[GetEdgeIndex(row, col)] = label;
                }
                else
                {
                    border.Content = new Label
                    {
                        FontAttributes = FontAttributes.Bold,
                        FontSize = 24,
                        HorizontalOptions = LayoutOptions.Center,
                        HorizontalTextAlignment = TextAlignment.Center,
                        Text = GetRandomLetter().ToString(),
                        TextColor = Colors.Black,
                        VerticalOptions = LayoutOptions.Center,
                        VerticalTextAlignment = TextAlignment.Center
                    };
                }

                FieldGrid.Add(border, col, row);
            }
        }
    }

    private static bool IsEdgeCenter(int row, int col)
    {
        int middle = GridSize / 2;
        return (row == 0 && col == middle)
            || (row == middle && col == 0)
            || (row == middle && col == GridSize - 1)
            || (row == GridSize - 1 && col == middle);
    }

    private static int GetEdgeIndex(int row, int col)
    {
        int middle = GridSize / 2;

        if (row == 0 && col == middle)
        {
            return 0;
        }

        if (row == middle && col == GridSize - 1)
        {
            return 1;
        }

        if (row == GridSize - 1 && col == middle)
        {
            return 2;
        }

        return 3;
    }

    private void ResetBoard()
    {
        foreach (var label in _edgeLabels)
        {
            if (label != null)
            {
                label.Text = "•";
                label.TextColor = Colors.Black;
            }
        }
    }

    private void ResetSession()
    {
        _currentLevel = 1;
        _roundsCount = 0;
        _correctRoundsCount = 0;
        _detectedMismatchCount = 0;
        _missedMismatchCount = 0;
        _falseAlarmCount = 0;
        _consecutiveCorrectRounds = 0;
        _currentRoundHasMismatch = false;
        _currentRoundScored = false;
        _statisticsSaved = false;

        UpdateSpeedByLevel();
        ResetBoard();
        UpdateStatsText();
    }

    private void UpdateSpeedByLevel()
    {
        _currentIntervalMilliseconds = _speedLevels[_currentLevel - 1];
    }

    private async Task RunExerciseLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
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

        char baseLetter = GetRandomLetter();
        bool hasMismatch = _random.NextDouble() < 0.3;
        int mismatchIndex = hasMismatch ? _random.Next(_edgeLabels.Length) : -1;

        _currentRoundHasMismatch = hasMismatch;

        for (int i = 0; i < _edgeLabels.Length; i++)
        {
            char currentLetter = i == mismatchIndex ? GetDifferentLetter(baseLetter) : baseLetter;
            _edgeLabels[i].Text = currentLetter.ToString();
            _edgeLabels[i].TextColor = Colors.Black;
        }

        StatusLabel.Text = "Следите за буквами и нажимайте «Ошибка» только при несовпадении.";
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
        return _letters[_random.Next(_letters.Length)];
    }

    private void FinalizeCurrentRound()
    {
        if (_currentRoundScored)
        {
            return;
        }

        if (_currentRoundHasMismatch)
        {
            _missedMismatchCount++;
            ApplyRoundResult(false, "Ошибка пропущена: одна буква отличалась.");
            return;
        }

        ApplyRoundResult(true, "Верно: все буквы совпадали.");
    }

    private void ApplyRoundResult(bool isCorrect, string message)
    {
        _currentRoundScored = true;

        if (isCorrect)
        {
            _correctRoundsCount++;
            _consecutiveCorrectRounds++;

            if (_consecutiveCorrectRounds >= CorrectRoundsToIncreaseLevel && _currentLevel < _speedLevels.Length)
            {
                _currentLevel++;
                _consecutiveCorrectRounds = 0;
                UpdateSpeedByLevel();
                message += $" Уровень повышен до {_currentLevel}.";
            }
        }
        else
        {
            _consecutiveCorrectRounds = 0;

            if (_currentLevel > 1)
            {
                _currentLevel--;
                UpdateSpeedByLevel();
                message += $" Уровень снижен до {_currentLevel}.";
            }
        }

        StatusLabel.Text = message;
        UpdateStatsText();
    }

    private void UpdateStatsText()
    {
        StatsLabel.Text = $"Уровень: {_currentLevel}, интервал: {_currentIntervalMilliseconds} мс. " +
                          $"Раундов: {_roundsCount}, правильных: {_correctRoundsCount}, " +
                          $"найдено ошибок: {_detectedMismatchCount}, пропущено: {_missedMismatchCount}, ложных: {_falseAlarmCount}.";
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
        finally
        {
            _isRunning = false;
            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;
            ErrorButton.IsEnabled = false;
        }
    }

    private async void OnStopClicked(object sender, EventArgs e)
    {
        await StopAndSaveAsync("Тренировка остановлена.");
    }

    private void OnErrorClicked(object sender, EventArgs e)
    {
        if (!_isRunning || _currentRoundScored)
        {
            return;
        }

        if (_currentRoundHasMismatch)
        {
            _detectedMismatchCount++;
            HighlightMismatch();
            ApplyRoundResult(true, "Верно: отличие замечено.");
            return;
        }

        _falseAlarmCount++;
        ApplyRoundResult(false, "Ложная тревога: все буквы были одинаковыми.");
    }

    private void HighlightMismatch()
    {
        string firstLetter = _edgeLabels[0].Text;

        foreach (var label in _edgeLabels)
        {
            label.TextColor = label.Text == firstLetter ? Colors.Black : Colors.Red;
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
        _isRunning = false;
        StatusLabel.Text = message;
        ResetBoard();
        await SaveStatisticsAsync();
    }

    protected override async void OnDisappearing()
    {
        await StopAndSaveAsync("Тренировка остановлена.");
        base.OnDisappearing();
    }
}
