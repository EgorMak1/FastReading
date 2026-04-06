using MauiApp1.Services;
namespace MauiApp1.Trainings;

public partial class RunningWordsPage : ContentPage
{
    private List<string> _words = new();
    private List<string> _answerOptions = new();
    private List<string> _distractorWords = new();

    private string _correctAnswer = string.Empty;
    private string _selectedAnswer = string.Empty;

    private readonly List<int> _speedLevels = new() { 700, 600, 500, 400, 300 };
    private int _currentSpeedLevel = 1;
    private int _wordDisplayMilliseconds = 700;

    private int _correctAnswersCount = 0;
    private int _totalAttempts = 0;
    private int _wrongAnswersCount = 0;

    private bool _isShowingWords = false;
    private bool _isAnswerSelection = false;
    private bool _isExerciseFinished = false;

    private readonly StatisticsService _statisticsService;

    public RunningWordsPage(StatisticsService statisticsService)
    {
        InitializeComponent();
        _statisticsService = statisticsService;
        InitializeExerciseData();
        UpdateSpeedByLevel();
        UpdateAnswerButtons();
        SetInitialState();
    }

    private void InitializeExerciseData()
    {
        _words = new List<string>
        {
            "дом",
            "лес",
            "река",
            "мост"
        };

        _distractorWords = new List<string>
        {
            "стол",
            "окно",
            "мяч",
            "снег",
            "книга",
            "лампа",
            "кошка",
            "дерево"
        };
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
            .OrderBy(x => random.Next())
            .Take(3)
            .ToList();

        _answerOptions = new List<string>(distractors)
        {
            _correctAnswer
        };

        _answerOptions = _answerOptions
            .OrderBy(x => random.Next())
            .ToList();
    }

    

    private void SetInitialState()
    {
        CurrentWordLabel.Text = "Нажмите кнопку, чтобы начать";
        StatusLabel.Text = $"После показа слов нужно выбрать последнее слово. " +
                   $"Уровень: {_currentSpeedLevel}, скорость: {_wordDisplayMilliseconds} мс. " +
                   $"Правильных ответов: {_correctAnswersCount} из {_totalAttempts}";

        SetAnswerButtonsEnabled(false);
    }

    private void UpdateAnswerButtons()
    {
        if (_answerOptions.Count >= 4)
        {
            AnswerButton1.Text = _answerOptions[0];
            AnswerButton2.Text = _answerOptions[1];
            AnswerButton3.Text = _answerOptions[2];
            AnswerButton4.Text = _answerOptions[3];
        }
    }

    private void SetAnswerButtonsEnabled(bool isEnabled)
    {
        AnswerButton1.IsEnabled = isEnabled;
        AnswerButton2.IsEnabled = isEnabled;
        AnswerButton3.IsEnabled = isEnabled;
        AnswerButton4.IsEnabled = isEnabled;
    }

    private async Task ShowWordsSequentiallyAsync()
    {
        foreach (var word in _words)
        {
            CurrentWordLabel.Text = word;
            await Task.Delay(_wordDisplayMilliseconds);
        }

        _isShowingWords = false;
        _isAnswerSelection = true;

        CurrentWordLabel.Text = "?";
        StatusLabel.Text = $"Выберите последнее показанное слово. Уровень: {_currentSpeedLevel}, скорость: {_wordDisplayMilliseconds} мс";

        SetAnswerButtonsEnabled(true);
        StartButton.IsEnabled = false;
    }

    private async void OnStartClicked(object sender, EventArgs e)
    {
        GenerateAnswerOptions();
        UpdateAnswerButtons();

        _isShowingWords = true;
        _isAnswerSelection = false;
        _isExerciseFinished = false;
        _selectedAnswer = string.Empty;

        StartButton.IsEnabled = false;
        SetAnswerButtonsEnabled(false);

        StatusLabel.Text = $"Смотрите на слова и запоминайте последнее. " +
                   $"Уровень: {_currentSpeedLevel}, скорость: {_wordDisplayMilliseconds} мс. " +
                   $"Правильных ответов: {_correctAnswersCount} из {_totalAttempts}";

        await ShowWordsSequentiallyAsync();
    }

    private void OnAnswerClicked(object sender, EventArgs e)
    {
        if (!_isAnswerSelection)
            return;

        if (sender is not Button button)
            return;

        _selectedAnswer = button.Text;

        bool isCorrect = _selectedAnswer == _correctAnswer;

        _totalAttempts++;

        if (isCorrect)
        {
            _correctAnswersCount++;
        }
        else
        {
            _wrongAnswersCount++;
        }

        _isAnswerSelection = false;
        _isExerciseFinished = true;

        if (isCorrect)
        {
            IncreaseSpeedLevel();
        }
        else
        {
            DecreaseSpeedLevel();
        }

        SetAnswerButtonsEnabled(false);
        StartButton.IsEnabled = true;

        CurrentWordLabel.Text = _correctAnswer;

        if (isCorrect)
        {
            StatusLabel.Text = $"Верно! Вы правильно выбрали последнее слово. " +
                               $"Новый уровень: {_currentSpeedLevel}, новая скорость: {_wordDisplayMilliseconds} мс. " +
                               $"Правильных ответов: {_correctAnswersCount} из {_totalAttempts}";
        }
        else
        {
            StatusLabel.Text = $"Неверно. Правильный ответ: {_correctAnswer}. " +
                               $"Новый уровень: {_currentSpeedLevel}, новая скорость: {_wordDisplayMilliseconds} мс. " +
                               $"Правильных ответов: {_correctAnswersCount} из {_totalAttempts}";
        }
    }
    protected override async void OnDisappearing()
    {
        base.OnDisappearing();

        // если пользователь ничего не сделал — не сохраняем
        if (_totalAttempts == 0)
            return;

        double accuracy = (double)_correctAnswersCount / _totalAttempts * 100;

        var result = new
        {
            TotalAttempts = _totalAttempts,
            CorrectAnswers = _correctAnswersCount,
            WrongAnswers = _wrongAnswersCount,
            AccuracyPercent = accuracy,
            FinalLevel = _currentSpeedLevel,
            FinalSpeedMilliseconds = _wordDisplayMilliseconds
        };

        await _statisticsService.SaveRunningWordsResultAsync(result);
    }
}