namespace MauiApp1.Trainings;

public partial class RunningWordsPage : ContentPage
{
    private List<string> _words = new();
    private List<string> _answerOptions = new();

    private string _correctAnswer = string.Empty;
    private string _selectedAnswer = string.Empty;

    private int _difficultyLevel = 1;
    private int _wordDisplayMilliseconds = 1000;

    private bool _isShowingWords = false;
    private bool _isAnswerSelection = false;
    private bool _isExerciseFinished = false;

    public RunningWordsPage()
    {
        InitializeComponent();
        InitializeExerciseData();
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

        _answerOptions = new List<string>
        {
            "река",
            "стол",
            "окно",
            "мяч"
        };

        _correctAnswer = "река";
    }

    private void SetInitialState()
    {
        CurrentWordLabel.Text = "Нажмите кнопку, чтобы начать";
        StatusLabel.Text = "После показа слов нужно выбрать последнее слово";

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

        _correctAnswer = _words.Last();

        _isShowingWords = false;
        _isAnswerSelection = true;

        CurrentWordLabel.Text = "?";
        StatusLabel.Text = "Выберите последнее показанное слово";

        SetAnswerButtonsEnabled(true);
        StartButton.IsEnabled = false;
    }

    private async void OnStartClicked(object sender, EventArgs e)
    {
        _isShowingWords = true;
        _isAnswerSelection = false;
        _isExerciseFinished = false;
        _selectedAnswer = string.Empty;

        StartButton.IsEnabled = false;
        SetAnswerButtonsEnabled(false);

        StatusLabel.Text = "Смотрите на слова и запоминайте последнее";

        await ShowWordsSequentiallyAsync();
    }

    private void OnAnswerClicked(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            _selectedAnswer = button.Text;
            StatusLabel.Text = $"Вы выбрали: {_selectedAnswer}";
        }
    }
}