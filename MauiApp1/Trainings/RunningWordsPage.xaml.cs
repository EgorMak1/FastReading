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
}