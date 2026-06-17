namespace MauiApp1.Trainings;

public sealed class ExerciseIntroPage : ContentPage
{
    private readonly Func<Page> _pageFactory;

    public ExerciseIntroPage(
        string title,
        string subtitle,
        IReadOnlyList<string> instructions,
        string difficultyHint,
        Func<Page> pageFactory)
    {
        _pageFactory = pageFactory;
        Title = title;

        var instructionsText = string.Join(Environment.NewLine, instructions.Select(item => $"• {item}"));

        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = 20,
                Spacing = 16,
                HorizontalOptions = LayoutOptions.Center,
                MaximumWidthRequest = 1040,
                Children =
                {
                    new Label
                    {
                        Text = title,
                        FontSize = 28,
                        FontAttributes = FontAttributes.Bold,
                        HorizontalOptions = LayoutOptions.Center
                    },
                    new Label
                    {
                        Text = subtitle,
                        FontSize = 15,
                        TextColor = Colors.Gray,
                        HorizontalTextAlignment = TextAlignment.Center
                    },
                    new Border
                    {
                        Stroke = Color.FromArgb("#D0D0D0"),
                        StrokeThickness = 1,
                        Padding = 14,
                        Content = new VerticalStackLayout
                        {
                            Spacing = 10,
                            Children =
                            {
                                new Label
                                {
                                    Text = "Как выполнять",
                                    FontSize = 18,
                                    FontAttributes = FontAttributes.Bold
                                },
                                new Label
                                {
                                    Text = instructionsText,
                                    FontSize = 15,
                                    LineHeight = 1.2
                                }
                            }
                        }
                    },
                    new Border
                    {
                        Stroke = Color.FromArgb("#D0D0D0"),
                        StrokeThickness = 1,
                        Padding = 14,
                        Content = new VerticalStackLayout
                        {
                            Spacing = 10,
                            Children =
                            {
                                new Label
                                {
                                    Text = "Сложность",
                                    FontSize = 18,
                                    FontAttributes = FontAttributes.Bold
                                },
                                new Label
                                {
                                    Text = difficultyHint,
                                    FontSize = 15,
                                    TextColor = Color.FromArgb("#5C6BC0")
                                }
                            }
                        }
                    },
                    new Button
                    {
                        Text = "Перейти к упражнению",
                        Command = new Command(async () => await Navigation.PushAsync(_pageFactory()))
                    }
                }
            }
        };
    }
}
