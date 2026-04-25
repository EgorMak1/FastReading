using MauiApp1.Services;

namespace MauiApp1.Trainings;

public partial class RunningWordsPage : ContentPage
{
    private const int DefaultWordDisplayMilliseconds = 500;
    private const int MinimumWordDisplayMilliseconds = 50;
    private const int MaximumWordDisplayMilliseconds = 500;
    private const int SpeedStepMilliseconds = 50;
    private const int WordsPerRound = 4;

    private readonly List<string> _wordPool = [];
    private readonly StatisticsService _statisticsService;
    private readonly Random _random = new();

    private List<string> _currentSequence = [];
    private List<string> _answerOptions = [];
    private CancellationTokenSource? _displayCancellation;

    private string _correctAnswer = string.Empty;
    private string _selectedAnswer = string.Empty;

    private int _wordDisplayMilliseconds = DefaultWordDisplayMilliseconds;
    private int _recommendedSpeedMilliseconds = DefaultWordDisplayMilliseconds;
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
        await InitializeSpeedAsync();
        _wordDisplayMilliseconds = _recommendedSpeedMilliseconds;
        UpdateDifficultyLabel();
        SetInitialState();
    }

    private async Task InitializeSpeedAsync()
    {
        try
        {
            var results = await _statisticsService.GetRunningWordsResultsAsync();
            var lastResult = results.LastOrDefault();

            if (lastResult != null)
            {
                _recommendedSpeedMilliseconds = NormalizeSpeed(lastResult.FinalSpeedMilliseconds);
            }
        }
        catch
        {
            _recommendedSpeedMilliseconds = DefaultWordDisplayMilliseconds;
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

    private void GenerateWordsSequence()
    {
        _currentSequence = _wordPool
            .OrderBy(_ => _random.Next())
            .Take(WordsPerRound)
            .ToList();

        _correctAnswer = _currentSequence.Last();
    }

    private void GenerateAnswerOptions()
    {
        var distractors = _wordPool
            .Where(word => word != _correctAnswer && !_currentSequence.Contains(word))
            .OrderBy(_ => _random.Next())
            .Take(3)
            .ToList();

        _answerOptions = new List<string>(distractors) { _correctAnswer }
            .OrderBy(_ => _random.Next())
            .ToList();
    }

    private void SetInitialState()
    {
        CurrentWordLabel.Text = "Нажмите старт";
        SetAnswerButtonsEnabled(false);
        UpdateDifficultyLabel();
        UpdateStatusText("Подготовка к раунду.");
    }

    private void UpdateDifficultyLabel()
    {
        DifficultyLabel.Text = $"Скорость: {_wordDisplayMilliseconds} мс. Старт по истории: {_recommendedSpeedMilliseconds} мс.";
    }

    private void UpdateStatusText(string message)
    {
        StatusLabel.Text = $"{message} Верных ответов: {_correctAnswersCount} из {_totalAttempts}.";
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
        UpdateStatusText("Выберите последнее слово.");
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
        UpdateStatusText("Смотрите на слова.");

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
            _wordDisplayMilliseconds = NormalizeSpeed(_wordDisplayMilliseconds - SpeedStepMilliseconds);
        }
        else
        {
            _wrongAnswersCount++;
            _wordDisplayMilliseconds = NormalizeSpeed(_wordDisplayMilliseconds + SpeedStepMilliseconds);
        }

        _recommendedSpeedMilliseconds = _wordDisplayMilliseconds;
        _isAnswerSelection = false;

        SetAnswerButtonsEnabled(false);
        StartButton.IsEnabled = true;
        CurrentWordLabel.Text = _correctAnswer;
        UpdateDifficultyLabel();

        if (isCorrect)
        {
            UpdateStatusText("Верно.");
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
            FinalLevel = CalculateSpeedBand(_wordDisplayMilliseconds),
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

    private static int CalculateSpeedBand(int speedMilliseconds)
    {
        return speedMilliseconds switch
        {
            <= 100 => 5,
            <= 200 => 4,
            <= 300 => 3,
            <= 400 => 2,
            _ => 1
        };
    }

    private static int NormalizeSpeed(int speedMilliseconds)
    {
        return Math.Clamp(speedMilliseconds, MinimumWordDisplayMilliseconds, MaximumWordDisplayMilliseconds);
    }
}
