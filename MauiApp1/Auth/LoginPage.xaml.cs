using MauiApp1.Services;

namespace MauiApp1.Auth
{
    public partial class LoginPage : ContentPage
    {
        private readonly AuthService _auth;
        private bool _savedTokenChecked;

        public LoginPage(AuthService auth)
        {
            InitializeComponent();
            _auth = auth;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (_savedTokenChecked)
            {
                return;
            }

            _savedTokenChecked = true;

            if (await _auth.TryApplySavedTokenAsync())
            {
                NavigateToMainPage();
            }
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            var email = EmailEntry.Text?.Trim();
            var password = PasswordEntry.Text;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("Ошибка", "Введите email или логин и пароль.", "ОК");
                return;
            }

            try
            {
                var token = await _auth.LoginAsync(email, password);

                if (string.IsNullOrWhiteSpace(token))
                {
                    await DisplayAlert("Ошибка", "Неверный email, логин или пароль.", "ОК");
                    return;
                }

                NavigateToMainPage();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", ex.Message, "ОК");
            }
        }

        private async void OnGoToRegisterClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(
                App.Current!.Handler!.MauiContext!.Services
                    .GetRequiredService<RegisterPage>());
        }

        private void NavigateToMainPage()
        {
            var services = App.Current!.Handler!.MauiContext!.Services;
            var mainPage = services.GetRequiredService<MainPage>();

            var window = Window ?? Application.Current!.Windows[0];
            window.Page = new NavigationPage(mainPage);
        }
    }
}
