using MauiApp1.Services;
using MauiApp1.Statistics;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace MauiApp1.Trainings;

public partial class WordErasingPage : ContentPage
{
    private const int DefaultWpm = 180;
    private const int MinWpm = 60;
    private const int MaxWpm = 400;
    private const int SessionDurationSeconds = 60;
    private const int QuestionCount = 5;

    private static readonly Regex TokenRegex = new(
        @"[\p{L}\p{N}]+(?:-[\p{L}\p{N}]+)*|\s+|[^\p{L}\p{N}\s]+",
        RegexOptions.Compiled);

    private readonly StatisticsService _statisticsService;
    private readonly bool _startImmediately;
    private IReadOnlyList<WordErasingTextDefinition> _texts = [];
    private readonly List<WordToken> _tokens = [];

    private CancellationTokenSource? _readingCancellation;
    private Stopwatch? _readingStopwatch;
    private WordErasingTextDefinition? _currentText;
    private WordErasingCompletionType _completionType;

    private int _currentWpm = DefaultWpm;
    private int _speedBeforeAttempt = DefaultWpm;
    private int _currentTextIndex;
    private int _currentQuestionIndex;
    private int _correctAnswers;
    private int _fullWordsErased;
    private int _lastRenderedPartialLetters = -1;
    private bool _isInitialized;
    private bool _isReading;

    public WordErasingPage(StatisticsService statisticsService, bool startImmediately = false)
    {
        InitializeComponent();
        _statisticsService = statisticsService;
        _startImmediately = startImmediately;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_isInitialized)
        {
            return;
        }

        _isInitialized = true;
        await InitializeSessionAsync();
    }

    private async Task InitializeSessionAsync()
    {
        _texts = await WordErasingContent.GetTextsAsync();

        List<WordErasingResultDto> results;
        try
        {
            results = await _statisticsService.GetWordErasingResultsAsync();
        }
        catch
        {
            results = [];
        }

        _currentWpm = NormalizeWpm(results.Count > 0 ? results.Last().SpeedAfterWpm : DefaultWpm);
        _currentTextIndex = _texts.Count == 0 ? 0 : results.Count % _texts.Count;
        _currentText = _texts.ElementAtOrDefault(_currentTextIndex);
        if (_currentText == null)
        {
            return;
        }

        _tokens.Clear();
        _tokens.AddRange(Tokenize(_currentText.Content));

        PreparationTitleLabel.Text = _currentText.Title;
        PreparationInfoLabel.Text = $"Текст {_currentTextIndex + 1} из {_texts.Count}. После чтения будет {QuestionCount} вопросов.";
        SpeedLabel.Text = $"Текущая скорость: {_currentWpm} WPM";
        DifficultyLabel.Text = $"Диапазон сложности: {GetSpeedBandText(_currentWpm)}. В следующей попытке скорость изменится по результату ответов.";

        if (_startImmediately)
        {
            await StartReadingAttemptAsync();
            return;
        }

        ShowPreparationState();
    }

    private void ShowPreparationState()
    {
        PreparationLayout.IsVisible = true;
        ReadingLayout.IsVisible = false;
        QuestionLayout.IsVisible = false;
    }

    private void ShowReadingState()
    {
        PreparationLayout.IsVisible = false;
        ReadingLayout.IsVisible = true;
        QuestionLayout.IsVisible = false;
    }

    private void ShowQuestionState()
    {
        PreparationLayout.IsVisible = false;
        ReadingLayout.IsVisible = false;
        QuestionLayout.IsVisible = true;
    }

    private async Task StartReadingAttemptAsync()
    {
        if (_isReading)
        {
            return;
        }

        if (_currentText == null)
        {
            _currentTextIndex = 0;
            _currentText = _texts.FirstOrDefault();
            if (_currentText == null)
            {
                return;
            }

            _tokens.Clear();
            _tokens.AddRange(Tokenize(_currentText.Content));
        }

        _speedBeforeAttempt = _currentWpm;
        _completionType = WordErasingCompletionType.Timer;
        _currentQuestionIndex = 0;
        _correctAnswers = 0;
        _fullWordsErased = 0;
        _lastRenderedPartialLetters = -1;
        _isReading = true;

        ReadyButton.IsEnabled = true;
        ReadingStatusLabel.Text = "Читайте текст. Кнопка «Готово» доступна в любой момент.";
        ReadingTextLabel.Text = _currentText.Content;
        TimerLabel.Text = FormatRemainingTime(SessionDurationSeconds);
        TimerProgressBar.Progress = 1;

        ShowReadingState();

        _readingCancellation?.Cancel();
        _readingCancellation = new CancellationTokenSource();
        _readingStopwatch = Stopwatch.StartNew();
        Dispatcher.Dispatch(() => _ = TextScrollView.ScrollToAsync(0, 0, false));

        try
        {
            await RunReadingLoopAsync(_readingCancellation.Token);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async void OnStartClicked(object sender, EventArgs e)
    {
        await StartReadingAttemptAsync();
    }

    private async Task RunReadingLoopAsync(CancellationToken cancellationToken)
    {
        double msPerWord = 60000d / _currentWpm;

        while (!cancellationToken.IsCancellationRequested)
        {
            double elapsedMs = _readingStopwatch?.Elapsed.TotalMilliseconds ?? 0;
            int remainingSeconds = Math.Max(0, SessionDurationSeconds - (int)Math.Floor(elapsedMs / 1000d));
            TimerLabel.Text = FormatRemainingTime(remainingSeconds);
            TimerProgressBar.Progress = Math.Clamp(1d - elapsedMs / (SessionDurationSeconds * 1000d), 0d, 1d);

            double wordProgress = elapsedMs / msPerWord;
            int fullWordsErased = Math.Min((int)Math.Floor(wordProgress), GetTotalWordCount());
            int partialLetters = CalculatePartialLetters(wordProgress, fullWordsErased);

            if (fullWordsErased != _fullWordsErased || partialLetters != _lastRenderedPartialLetters)
            {
                _fullWordsErased = fullWordsErased;
                _lastRenderedPartialLetters = partialLetters;
                ReadingTextLabel.Text = BuildRenderedText(fullWordsErased, partialLetters);
            }

            if (elapsedMs >= SessionDurationSeconds * 1000d)
            {
                _completionType = WordErasingCompletionType.Timer;
                _isReading = false;
                TimerLabel.Text = "00:00";
                TimerProgressBar.Progress = 0;
                await BeginQuestionsAsync();
                return;
            }

            await Task.Delay(50, cancellationToken);
        }
    }

    private async void OnStopClicked(object sender, EventArgs e)
    {
        if (!_isReading)
        {
            return;
        }

        _completionType = WordErasingCompletionType.Stop;
        _isReading = false;
        _readingCancellation?.Cancel();

        if (GetErasedPercent() < 50d)
        {
            await FinishAttemptAsync(0, true);
            return;
        }

        await BeginQuestionsAsync();
    }

    private async void OnReadyClicked(object sender, EventArgs e)
    {
        if (!_isReading)
        {
            return;
        }

        _completionType = WordErasingCompletionType.Ready;
        _isReading = false;
        _readingCancellation?.Cancel();
        await BeginQuestionsAsync();
    }

    private Task BeginQuestionsAsync()
    {
        if (_currentText == null)
        {
            return Task.CompletedTask;
        }

        _currentQuestionIndex = 0;
        ShowQuestionState();
        BindCurrentQuestion();
        return Task.CompletedTask;
    }

    private void BindCurrentQuestion()
    {
        if (_currentText == null)
        {
            return;
        }

        var question = _currentText.Questions[_currentQuestionIndex];
        QuestionProgressLabel.Text = $"Вопрос {_currentQuestionIndex + 1} из {QuestionCount}";
        QuestionPromptLabel.Text = question.Prompt;
        AnswerButton1.Text = question.Options[0];
        AnswerButton2.Text = question.Options[1];
        AnswerButton3.Text = question.Options[2];
        AnswerButton4.Text = question.Options[3];
    }

    private async void OnAnswerClicked(object sender, EventArgs e)
    {
        if (_currentText == null || sender is not Button button)
        {
            return;
        }

        if (!int.TryParse(button.CommandParameter?.ToString(), out int selectedIndex))
        {
            return;
        }

        var question = _currentText.Questions[_currentQuestionIndex];
        if (selectedIndex == question.CorrectOptionIndex)
        {
            _correctAnswers++;
        }

        _currentQuestionIndex++;

        if (_currentQuestionIndex >= QuestionCount)
        {
            await FinishAttemptAsync(_correctAnswers, false);
            return;
        }

        BindCurrentQuestion();
    }

    private async Task FinishAttemptAsync(int correctAnswers, bool questionsSkipped)
    {
        _readingCancellation?.Cancel();
        _isReading = false;

        int speedDelta = questionsSkipped ? -15 : CalculateSpeedDelta(correctAnswers);
        int speedAfter = NormalizeWpm(_speedBeforeAttempt + speedDelta);
        double accuracyPercent = questionsSkipped ? 0 : correctAnswers / (double)QuestionCount * 100d;

        var request = new WordErasingResultRequest
        {
            TextId = _currentText?.Id ?? string.Empty,
            TextTitle = _currentText?.Title ?? string.Empty,
            SpeedBeforeWpm = _speedBeforeAttempt,
            SpeedAfterWpm = speedAfter,
            SpeedDelta = speedAfter - _speedBeforeAttempt,
            CompletionType = _completionType.ToString(),
            CorrectAnswers = correctAnswers,
            TotalQuestions = QuestionCount,
            QuestionsSkipped = questionsSkipped,
            AccuracyPercent = accuracyPercent,
            ErasedWords = _fullWordsErased,
            TotalWords = GetTotalWordCount()
        };

        await _statisticsService.SaveWordErasingResultAsync(request);
        _currentWpm = speedAfter;

        var statisticsPage = App.Current!.Handler!.MauiContext!.Services.GetRequiredService<WordErasingStatisticsPage>();
        await Navigation.PushAsync(statisticsPage);
        Navigation.RemovePage(this);
    }

    private static int CalculateSpeedDelta(int correctAnswers)
    {
        return correctAnswers switch
        {
            5 => 15,
            4 => 10,
            3 => 0,
            2 => -10,
            _ => -15
        };
    }

    private double GetErasedPercent()
    {
        int totalWords = GetTotalWordCount();
        return totalWords == 0 ? 0 : _fullWordsErased * 100d / totalWords;
    }

    private int GetTotalWordCount()
    {
        return _tokens.Count(token => token.IsWord);
    }

    private int CalculatePartialLetters(double wordProgress, int fullWordsErased)
    {
        if (fullWordsErased >= GetTotalWordCount())
        {
            return 0;
        }

        double fraction = wordProgress - Math.Floor(wordProgress);
        var currentWord = _tokens.FirstOrDefault(token => token.IsWord && token.WordIndex == fullWordsErased);

        return currentWord == null
            ? 0
            : Math.Min(currentWord.Text.Length, (int)Math.Floor(fraction * currentWord.Text.Length));
    }

    private string BuildRenderedText(int fullWordsErased, int partialLetters)
    {
        var builder = new StringBuilder();

        foreach (var token in _tokens)
        {
            if (!token.IsWord)
            {
                builder.Append(token.Text);
                continue;
            }

            if (token.WordIndex < fullWordsErased)
            {
                builder.Append(new string('_', token.Text.Length));
                continue;
            }

            if (token.WordIndex == fullWordsErased && partialLetters > 0)
            {
                int lettersToErase = Math.Min(partialLetters, token.Text.Length);
                builder.Append(new string('_', lettersToErase));
                builder.Append(token.Text[lettersToErase..]);
                continue;
            }

            builder.Append(token.Text);
        }

        return builder.ToString();
    }

    private static List<WordToken> Tokenize(string content)
    {
        var tokens = new List<WordToken>();
        var matches = TokenRegex.Matches(content);
        int wordIndex = 0;

        foreach (Match match in matches)
        {
            string tokenText = match.Value;
            bool isWord = char.IsLetterOrDigit(tokenText[0]);
            tokens.Add(new WordToken(tokenText, isWord, isWord ? wordIndex : -1));

            if (isWord)
            {
                wordIndex++;
            }
        }

        return tokens;
    }

    private void OnTextScrolled(object? sender, ScrolledEventArgs e)
    {
        if (_isReading)
        {
            ReadyButton.IsEnabled = true;
        }
    }

    private static string FormatRemainingTime(int remainingSeconds)
    {
        var time = TimeSpan.FromSeconds(Math.Max(0, remainingSeconds));
        return $"{time.Minutes:00}:{time.Seconds:00}";
    }

    private static int NormalizeWpm(int wpm)
    {
        return Math.Clamp(wpm, MinWpm, MaxWpm);
    }

    private static string GetSpeedBandText(int wpm)
    {
        return wpm switch
        {
            <= 160 => "уровень 1 (базовый темп)",
            <= 220 => "уровень 2 (устойчивый темп)",
            <= 280 => "уровень 3 (ускоренное чтение)",
            <= 340 => "уровень 4 (высокий темп)",
            _ => "уровень 5 (максимальный темп)"
        };
    }

    protected override void OnDisappearing()
    {
        _readingCancellation?.Cancel();
        _isReading = false;
        base.OnDisappearing();
    }

    private sealed record WordToken(string Text, bool IsWord, int WordIndex);
}

public enum WordErasingCompletionType
{
    Stop,
    Ready,
    Timer
}
