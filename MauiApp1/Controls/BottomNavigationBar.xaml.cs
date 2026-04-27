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

    private void OnHomeClicked(object sender, EventArgs e)
    {
        NavigateTo<MainPage>("Home");
    }

    private void OnTrainingClicked(object sender, EventArgs e)
    {
        NavigateTo<ExerciseSelectionPage>("Training");
    }

    private void OnStatisticsClicked(object sender, EventArgs e)
    {
        NavigateTo<SelectionStatisticsPage>("Statistics");
    }

    private void OnProfileClicked(object sender, EventArgs e)
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
        if (HomeButton == null)
        {
            return;
        }

        bool isDark = Application.Current?.RequestedTheme == AppTheme.Dark;
        var activeBackground = isDark ? Color.FromArgb("#93C5FD") : Color.FromArgb("#2563EB");
        var activeText = isDark ? Color.FromArgb("#111827") : Colors.White;
        var inactiveText = isDark ? Color.FromArgb("#D1D5DB") : Color.FromArgb("#6B7280");

        ApplyButtonState(HomeButton, ActiveTab == "Home", activeBackground, activeText, inactiveText);
        ApplyButtonState(TrainingButton, ActiveTab == "Training", activeBackground, activeText, inactiveText);
        ApplyButtonState(StatisticsButton, ActiveTab == "Statistics", activeBackground, activeText, inactiveText);
        ApplyButtonState(ProfileButton, ActiveTab == "Profile", activeBackground, activeText, inactiveText);
    }

    private static void ApplyButtonState(Button button, bool isActive, Color activeBackground, Color activeText, Color inactiveText)
    {
        button.BackgroundColor = isActive ? activeBackground : Colors.Transparent;
        button.TextColor = isActive ? activeText : inactiveText;
        button.BorderColor = Colors.Transparent;
        button.BorderWidth = 0;
        button.CornerRadius = 8;
        button.FontSize = 13;
        button.MinimumHeightRequest = 42;
        button.Padding = new Thickness(10, 8);
    }
}
