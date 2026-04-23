using MauiApp1.Services;

namespace MauiApp1.Trainings;

public partial class RunningWordsPage : ContentPage
{
    private static readonly IReadOnlyList<RunningWordsDifficultyConfig> DifficultyLevels =
    [
        new(1, 700, 3, false),
        new(2, 600, 4, false),
        new(3, 500, 5, false),
        new(4, 400, 6, true),
        new(5, 300, 7, true)
    ];

    private readonly List<string> _wordPool = [];
    private readonly StatisticsService _statisticsService;
    private readonly Random _random = new();

    private List<string> _currentSequence = [];
    private List<string> _answerOptions = [];
    private CancellationTokenSource? _displayCancellation;

    private string _correctAnswer = string.Empty;
    private string _selectedAnswer = string.Empty;

    private int _currentSpeedLevel = 1;
    private int _recommendedLevel = 1;
    private int _wordDisplayMilliseconds = 700;
    private int _wordsPerRound = 3;
    private int _correctAnswersCount;
    private int _totalAttempts;
    private int _wrongAnswersCount;

    private bool _isShowingWords;
    private bool _isAnswerSelection;
    private bool _statisticsSaved;
    private bool _isInitialized;

    public RunningWordsPage(StatisticsService statisticsService)
    {
        InitializeComponent();
        _statisticsService = statisticsService;
        InitializeExerciseData();
        SetInitialState();
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
        SetInitialState();
    }

    private async Task InitializeDifficultyAsync()
    {
        try
        {
            var results = await _statisticsService.GetRunningWordsResultsAsync();
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

    private void InitializeExerciseData()
    {
        _wordPool.AddRange(
        [
            "дом", "лес", "река", "мост", "стол", "окно", "мяч", "снег",
            "книга", "лампа", "кошка", "дерево", "поле", "ветер", "дождь",
            "ручей", "берег", "трава", "пламя", "школа", "маска", "карта",
            "город", "капля", "ветка", "птица", "звезда", "облако"
        ]);
    }

    private void ApplyLevel(int level)
    {
        _currentSpeedLevel = ClampLevel(level);
        var config = DifficultyLevels[_currentSpeedLevel - 1];
        _wordDisplayMilliseconds = config.WordDisplayMilliseconds;
        _wordsPerRound = config.WordsPerRound;
    }

    private void IncreaseSpeedLevel()
    {
        ApplyLevel(_currentSpeedLevel + 1);
    }

    private void DecreaseSpeedLevel()
    {
        ApplyLevel(_currentSpeedLevel - 1);
    }

    private void GenerateWordsSequence()
    {
        _currentSequence = _wordPool
            .OrderBy(_ => _random.Next())
            .Take(_wordsPerRound)
            .ToList();

        _correctAnswer = _currentSequence.Last();
    }

    private void GenerateAnswerOptions()
    {
        var config = DifficultyLevels[_currentSpeedLevel - 1];

        IEnumerable<string> distractorPool = _wordPool.Where(word => word != _correctAnswer && !_currentSequence.Contains(word));

        if (config.UseSimilarDistractors)
        {
            var similar = distractorPool
                .Where(word => word.Length == _correctAnswer.Length || word[0] == _correctAnswer[0])
                .ToList();

            if (similar.Count >= 3)
            {
                distractorPool = similar;
            }
        }

        var distractors = distractorPool
            .OrderBy(_ => _random.Next())
            .Take(3)
            .ToList();

        _answerOptions = new List<string>(distractors) { _correctAnswer }
            .OrderBy(_ => _random.Next())
            .ToList();
    }

    private void SetInitialState()
    {
        CurrentWordLabel.Text = "Нажмите кнопку, чтобы начать";
        SetAnswerButtonsEnabled(false);
        UpdateDifficultyLabel();
        UpdateStatusText("После показа слов нужно выбрать последнее слово.");
    }

    private void UpdateDifficultyLabel()
    {
        DifficultyLabel.Text = $"Уровень {_currentSpeedLevel}: {_wordsPerRound} слов, скорость {_wordDisplayMilliseconds} мс. Рекомендованный стартовый уровень: {_recommendedLevel}.";
    }

    private void UpdateStatusText(string message)
    {
        StatusLabel.Text = $"{message} Уровень: {_currentSpeedLevel}, скорость: {_wordDisplayMilliseconds} мс, слов в серии: {_wordsPerRound}. " +
                           $"Правильных ответов: {_correctAnswersCount} из {_totalAttempts}";
    }

    private void UpdateAnswerButtons()
    {
        if (_answerOptions.Count < 4)
        {
            return;
        }

        AnswerButton1.Text = _answerOptions[0];
        AnswerButton2.Text = _answerOptions[1];
        AnswerButton3.Text = _answerOptions[2];
        AnswerButton4.Text = _answerOptions[3];
    }

    private void SetAnswerButtonsEnabled(bool isEnabled)
    {
        AnswerButton1.IsEnabled = isEnabled;
        AnswerButton2.IsEnabled = isEnabled;
        AnswerButton3.IsEnabled = isEnabled;
        AnswerButton4.IsEnabled = isEnabled;
    }

    private async Task ShowWordsSequentiallyAsync(CancellationToken cancellationToken)
    {
        foreach (var word in _currentSequence)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CurrentWordLabel.Text = word;
            await Task.Delay(_wordDisplayMilliseconds, cancellationToken);
        }

        _isShowingWords = false;
        _isAnswerSelection = true;

        CurrentWordLabel.Text = "?";
        UpdateStatusText("Выберите последнее показанное слово.");
        SetAnswerButtonsEnabled(true);
        StartButton.IsEnabled = false;
    }

    private async void OnStartClicked(object sender, EventArgs e)
    {
        if (_isShowingWords)
        {
            return;
        }

        GenerateWordsSequence();
        GenerateAnswerOptions();
        UpdateAnswerButtons();

        _displayCancellation?.Cancel();
        _displayCancellation = new CancellationTokenSource();
        _isShowingWords = true;
        _isAnswerSelection = false;
        _selectedAnswer = string.Empty;
        _statisticsSaved = false;

        StartButton.IsEnabled = false;
        SetAnswerButtonsEnabled(false);
        UpdateStatusText("Смотрите на слова и запоминайте последнее.");

        try
        {
            await ShowWordsSequentiallyAsync(_displayCancellation.Token);
        }
        catch (OperationCanceledException)
        {
            _isShowingWords = false;
        }
    }

    private void OnAnswerClicked(object sender, EventArgs e)
    {
        if (!_isAnswerSelection || sender is not Button button)
        {
            return;
        }

        _selectedAnswer = button.Text;
        bool isCorrect = _selectedAnswer == _correctAnswer;

        _totalAttempts++;

        if (isCorrect)
        {
            _correctAnswersCount++;
            IncreaseSpeedLevel();
        }
        else
        {
            _wrongAnswersCount++;
            DecreaseSpeedLevel();
        }

        _recommendedLevel = _currentSpeedLevel;
        _isAnswerSelection = false;

        SetAnswerButtonsEnabled(false);
        StartButton.IsEnabled = true;
        CurrentWordLabel.Text = _correctAnswer;
        UpdateDifficultyLabel();

        if (isCorrect)
        {
            UpdateStatusText("Верно! Вы правильно выбрали последнее слово.");
        }
        else
        {
            UpdateStatusText($"Неверно. Правильный ответ: {_correctAnswer}.");
        }
    }

    private async Task SaveStatisticsAsync()
    {
        if (_statisticsSaved || _totalAttempts == 0)
        {
            return;
        }

        var request = new RunningWordsResultRequest
        {
            TotalAttempts = _totalAttempts,
            CorrectAnswers = _correctAnswersCount,
            WrongAnswers = _wrongAnswersCount,
            AccuracyPercent = (double)_correctAnswersCount / _totalAttempts * 100,
            FinalLevel = _currentSpeedLevel,
            FinalSpeedMilliseconds = _wordDisplayMilliseconds
        };

        _statisticsSaved = await _statisticsService.SaveRunningWordsResultAsync(request);
    }

    protected override async void OnDisappearing()
    {
        _displayCancellation?.Cancel();
        _isShowingWords = false;
        _isAnswerSelection = false;
        SetAnswerButtonsEnabled(false);

        await SaveStatisticsAsync();
        base.OnDisappearing();
    }

    private static int ClampLevel(int level)
    {
        return Math.Clamp(level, 1, DifficultyLevels.Count);
    }

    private sealed record RunningWordsDifficultyConfig(
        int Level,
        int WordDisplayMilliseconds,
        int WordsPerRound,
        bool UseSimilarDistractors);
}
