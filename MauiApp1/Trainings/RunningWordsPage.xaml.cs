using MauiApp1.Services;

namespace MauiApp1.Trainings;

public partial class RunningWordsPage : ContentPage
{
    private readonly List<string> _words = new();
    private readonly List<string> _distractorWords = new();
    private readonly List<int> _speedLevels = new() { 700, 600, 500, 400, 300 };
    private readonly StatisticsService _statisticsService;

    private List<string> _answerOptions = new();
    private CancellationTokenSource? _displayCancellation;

    private string _correctAnswer = string.Empty;
    private string _selectedAnswer = string.Empty;

    private int _currentSpeedLevel = 1;
    private int _wordDisplayMilliseconds = 700;
    private int _correctAnswersCount;
    private int _totalAttempts;
    private int _wrongAnswersCount;

    private bool _isShowingWords;
    private bool _isAnswerSelection;
    private bool _statisticsSaved;

    public RunningWordsPage(StatisticsService statisticsService)
    {
        InitializeComponent();
        _statisticsService = statisticsService;
        InitializeExerciseData();
        UpdateSpeedByLevel();
        SetInitialState();
    }

    private void InitializeExerciseData()
    {
        _words.AddRange(new[] { "дом", "лес", "река", "мост" });
        _distractorWords.AddRange(new[] { "стол", "окно", "мяч", "снег", "книга", "лампа", "кошка", "дерево" });
    }

    private void UpdateSpeedByLevel()
    {
        _wordDisplayMilliseconds = _speedLevels[_currentSpeedLevel - 1];
    }

    private void IncreaseSpeedLevel()
    {
        if (_currentSpeedLevel < _speedLevels.Count)
        {
            _currentSpeedLevel++;
            UpdateSpeedByLevel();
        }
    }

    private void DecreaseSpeedLevel()
    {
        if (_currentSpeedLevel > 1)
        {
            _currentSpeedLevel--;
            UpdateSpeedByLevel();
        }
    }

    private void GenerateAnswerOptions()
    {
        _correctAnswer = _words.Last();

        var random = new Random();
        var distractors = _distractorWords
            .Where(word => word != _correctAnswer)
            .OrderBy(_ => random.Next())
            .Take(3)
            .ToList();

        _answerOptions = new List<string>(distractors) { _correctAnswer }
            .OrderBy(_ => random.Next())
            .ToList();
    }

    private void SetInitialState()
    {
        CurrentWordLabel.Text = "Нажмите кнопку, чтобы начать";
        SetAnswerButtonsEnabled(false);
        UpdateStatusText("После показа слов нужно выбрать последнее слово.");
    }

    private void UpdateStatusText(string message)
    {
        StatusLabel.Text = $"{message} Уровень: {_currentSpeedLevel}, скорость: {_wordDisplayMilliseconds} мс. " +
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
        foreach (var word in _words)
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

        _isAnswerSelection = false;

        SetAnswerButtonsEnabled(false);
        StartButton.IsEnabled = true;
        CurrentWordLabel.Text = _correctAnswer;

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
}
