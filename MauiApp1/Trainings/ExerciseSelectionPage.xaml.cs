using MauiApp1.Services;

namespace MauiApp1.Trainings;

public partial class ExerciseSelectionPage : ContentPage
{
    private readonly StatisticsService _statisticsService;

    public ExerciseSelectionPage(StatisticsService statisticsService)
    {
        InitializeComponent();
        _statisticsService = statisticsService;
    }

    private async void OnShulteTableClicked(object sender, EventArgs e)
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

    private async void OnRunningWordsClicked(object sender, EventArgs e)
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

    private async void OnFieldOfViewClicked(object sender, EventArgs e)
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

    private async void OnWordErasingClicked(object sender, EventArgs e)
    {
        var page = new ExerciseIntroPage(
            title: "Затирание слов",
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
