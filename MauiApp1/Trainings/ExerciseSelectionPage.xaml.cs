using MauiApp1.Services;

namespace MauiApp1.Trainings;

public partial class ExerciseSelectionPage : ContentPage
{
    private const int AccuracySampleSize = 10;

    private readonly StatisticsService _statisticsService;
    private CancellationTokenSource? _loadCancellation;

    public ExerciseSelectionPage(StatisticsService statisticsService)
    {
        InitializeComponent();
        _statisticsService = statisticsService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _loadCancellation?.Cancel();
        _loadCancellation = new CancellationTokenSource();
        await LoadExerciseSummaryAsync(_loadCancellation.Token);
    }

    protected override void OnDisappearing()
    {
        _loadCancellation?.Cancel();
        base.OnDisappearing();
    }

    private async Task LoadExerciseSummaryAsync(CancellationToken cancellationToken)
    {
        SetSummaryLoading();

        try
        {
            var runningWordsTask = _statisticsService.GetRunningWordsResultsAsync();
            var shulteTask = _statisticsService.GetShulteResultsAsync();
            var fieldOfViewTask = _statisticsService.GetFieldOfViewResultsAsync();
            var wordErasingTask = _statisticsService.GetWordErasingResultsAsync();

            await Task.WhenAll(runningWordsTask, shulteTask, fieldOfViewTask, wordErasingTask);

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            ApplyRunningWordsSummary(runningWordsTask.Result);
            ApplyShulteSummary(shulteTask.Result);
            ApplyFieldOfViewSummary(fieldOfViewTask.Result);
            ApplyWordErasingSummary(wordErasingTask.Result);
        }
        catch
        {
            SetSummaryEmpty();
        }
    }

    private void SetSummaryLoading()
    {
        RunningWordsAccuracyLabel.Text = "Загрузка...";
        RunningWordsRecordLabel.Text = "Загрузка...";
        ShulteAccuracyLabel.Text = "Загрузка...";
        ShulteRecordLabel.Text = "Загрузка...";
        FieldOfViewAccuracyLabel.Text = "Загрузка...";
        FieldOfViewRecordLabel.Text = "Загрузка...";
        WordErasingAccuracyLabel.Text = "Загрузка...";
        WordErasingRecordLabel.Text = "Загрузка...";
    }

    private void SetSummaryEmpty()
    {
        RunningWordsAccuracyLabel.Text = "Нет данных";
        RunningWordsRecordLabel.Text = "Нет данных";
        ShulteAccuracyLabel.Text = "Нет данных";
        ShulteRecordLabel.Text = "Нет данных";
        FieldOfViewAccuracyLabel.Text = "Нет данных";
        FieldOfViewRecordLabel.Text = "Нет данных";
        WordErasingAccuracyLabel.Text = "Нет данных";
        WordErasingRecordLabel.Text = "Нет данных";
    }

    private void ApplyRunningWordsSummary(List<RunningWordsResultDto> results)
    {
        RunningWordsAccuracyLabel.Text = FormatAccuracy(
            results
                .OrderByDescending(x => x.CompletedAt)
                .Take(AccuracySampleSize)
                .Select(x => x.AccuracyPercent));

        RunningWordsRecordLabel.Text = results.Count == 0
            ? "Нет данных"
            : $"{results.Min(x => x.FinalSpeedMilliseconds)} мс";
    }

    private void ApplyShulteSummary(List<ShulteResultDto> results)
    {
        ShulteAccuracyLabel.Text = FormatAccuracy(
            results
                .OrderByDescending(x => x.CompletedAt)
                .Take(AccuracySampleSize)
                .Select(CalculateShulteAccuracy));

        var validResults = results
            .Where(x => x.DurationSeconds > 0)
            .ToList();

        var hardestAttempt = validResults
            .OrderByDescending(x => x.LevelAfter)
            .ThenByDescending(x => x.GridSize)
            .ThenByDescending(x => x.NumbersCount)
            .FirstOrDefault();

        if (hardestAttempt == null)
        {
            ShulteRecordLabel.Text = "Нет данных";
            return;
        }

        var recordSeconds = validResults
            .Where(x =>
                x.LevelAfter == hardestAttempt.LevelAfter &&
                x.GridSize == hardestAttempt.GridSize &&
                x.NumbersCount == hardestAttempt.NumbersCount)
            .Min(x => x.DurationSeconds);

        ShulteRecordLabel.Text = recordSeconds <= 0
            ? "Нет данных"
            : $"{recordSeconds} сек";
    }

    private void ApplyFieldOfViewSummary(List<FieldOfViewResultDto> results)
    {
        FieldOfViewAccuracyLabel.Text = FormatAccuracy(
            results
                .OrderByDescending(x => x.CompletedAt)
                .Take(AccuracySampleSize)
                .Select(x => x.AccuracyPercent));

        FieldOfViewRecordLabel.Text = results.Count == 0
            ? "Нет данных"
            : $"{results.Max(x => x.FinalLevel)} ур.";
    }

    private void ApplyWordErasingSummary(List<WordErasingResultDto> results)
    {
        WordErasingAccuracyLabel.Text = FormatAccuracy(
            results
                .OrderByDescending(x => x.CompletedAt)
                .Take(AccuracySampleSize)
                .Select(x => x.AccuracyPercent));

        WordErasingRecordLabel.Text = results.Count == 0
            ? "Нет данных"
            : $"{results.Max(x => x.SpeedAfterWpm)} сл/мин";
    }

    private static double CalculateShulteAccuracy(ShulteResultDto result)
    {
        if (result.NumbersCount <= 0)
        {
            return 0;
        }

        return Math.Clamp(
            (result.NumbersCount - result.ErrorsCount) / (double)result.NumbersCount * 100,
            0,
            100);
    }

    private static string FormatAccuracy(IEnumerable<double> values)
    {
        var sample = values.ToList();
        return sample.Count == 0
            ? "Нет данных"
            : $"{sample.Average():F0}%";
    }

    private async void OnShulteTableTapped(object sender, TappedEventArgs e)
    {
        await OpenShulteTableAsync();
    }

    private async void OnRunningWordsTapped(object sender, TappedEventArgs e)
    {
        await OpenRunningWordsAsync();
    }

    private async void OnFieldOfViewTapped(object sender, TappedEventArgs e)
    {
        await OpenFieldOfViewAsync();
    }

    private async void OnWordErasingTapped(object sender, TappedEventArgs e)
    {
        await OpenWordErasingAsync();
    }

    private async Task OpenShulteTableAsync()
    {
        var page = new ExerciseIntroPage(
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

        await Navigation.PushAsync(page);
    }

    private async Task OpenRunningWordsAsync()
    {
        var page = new ExerciseIntroPage(
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

        await Navigation.PushAsync(page);
    }

    private async Task OpenFieldOfViewAsync()
    {
        var page = new ExerciseIntroPage(
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

        await Navigation.PushAsync(page);
    }

    private async Task OpenWordErasingAsync()
    {
        var page = new ExerciseIntroPage(
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

        await Navigation.PushAsync(page);
    }
}
