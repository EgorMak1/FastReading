using MauiApp1.Profile;
using MauiApp1.Statistics;
using MauiApp1.Trainings;

namespace MauiApp1.Controls;

public partial class BottomNavigationBar : ContentView
{
    public static readonly BindableProperty ActiveTabProperty = BindableProperty.Create(
        nameof(ActiveTab),
        typeof(string),
        typeof(BottomNavigationBar),
        string.Empty,
        propertyChanged: (bindable, _, _) => ((BottomNavigationBar)bindable).ApplyActiveState());

    public string ActiveTab
    {
        get => (string)GetValue(ActiveTabProperty);
        set => SetValue(ActiveTabProperty, value);
    }

    public BottomNavigationBar()
    {
        InitializeComponent();
        Loaded += (_, _) => ApplyActiveState();
        Unloaded += (_, _) => UnsubscribeFromThemeChanges();

        if (Application.Current != null)
        {
            Application.Current.RequestedThemeChanged += OnRequestedThemeChanged;
        }
    }

    private void OnRequestedThemeChanged(object? sender, AppThemeChangedEventArgs e)
    {
        ApplyActiveState();
    }

    private void UnsubscribeFromThemeChanges()
    {
        if (Application.Current != null)
        {
            Application.Current.RequestedThemeChanged -= OnRequestedThemeChanged;
        }
    }

    private void OnHomeClicked(object sender, TappedEventArgs e)
    {
        NavigateTo<MainPage>("Home");
    }

    private void OnTrainingClicked(object sender, TappedEventArgs e)
    {
        NavigateTo<ExerciseSelectionPage>("Training");
    }

    private void OnStatisticsClicked(object sender, TappedEventArgs e)
    {
        NavigateTo<SelectionStatisticsPage>("Statistics");
    }

    private void OnProfileClicked(object sender, TappedEventArgs e)
    {
        NavigateTo<ProfilePage>("Profile");
    }

    private void NavigateTo<TPage>(string tabName)
        where TPage : Page
    {
        if (ActiveTab == tabName)
        {
            return;
        }

        var services = Handler?.MauiContext?.Services
            ?? Application.Current?.Windows.FirstOrDefault()?.Handler?.MauiContext?.Services;

        if (services == null)
        {
            return;
        }

        var page = services.GetRequiredService<TPage>();
        var window = Application.Current?.Windows.FirstOrDefault();
        if (window == null)
        {
            return;
        }

        window.Page = new NavigationPage(page);
    }

    private void ApplyActiveState()
    {
        if (HomeTab == null)
        {
            return;
        }

        bool isDark = Application.Current?.RequestedTheme == AppTheme.Dark;
        var activeBackground = isDark ? Color.FromArgb("#93C5FD") : Color.FromArgb("#2563EB");
        var activeText = isDark ? Color.FromArgb("#111827") : Colors.White;
        var inactiveText = isDark ? Color.FromArgb("#D1D5DB") : Color.FromArgb("#6B7280");
        var inactiveBackground = Colors.Transparent;

        ApplyTabState(HomeTab, HomeIcon, HomeLabel, ActiveTab == "Home", activeBackground, inactiveBackground, activeText, inactiveText);
        ApplyTabState(TrainingTab, TrainingIcon, TrainingLabel, ActiveTab == "Training", activeBackground, inactiveBackground, activeText, inactiveText);
        ApplyTabState(StatisticsTab, StatisticsIcon, StatisticsLabel, ActiveTab == "Statistics", activeBackground, inactiveBackground, activeText, inactiveText);
        ApplyTabState(ProfileTab, ProfileIcon, ProfileLabel, ActiveTab == "Profile", activeBackground, inactiveBackground, activeText, inactiveText);
    }

    private static void ApplyTabState(
        Border tab,
        Microsoft.Maui.Controls.Shapes.Path icon,
        Label label,
        bool isActive,
        Color activeBackground,
        Color inactiveBackground,
        Color activeText,
        Color inactiveText)
    {
        var foreground = isActive ? activeText : inactiveText;
        tab.BackgroundColor = isActive ? activeBackground : inactiveBackground;
        label.TextColor = foreground;
        icon.Stroke = new SolidColorBrush(foreground);
    }
}
