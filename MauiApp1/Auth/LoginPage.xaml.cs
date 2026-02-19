using MauiApp1.Services;

namespace MauiApp1.Auth
{
    public partial class LoginPage : ContentPage
    {
        private readonly AuthService _auth;

        public LoginPage(AuthService auth)
        {
            InitializeComponent();
            _auth = auth;
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            var email = EmailEntry.Text?.Trim();
            var password = PasswordEntry.Text;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("Ошибка", "Введите Email и Password.", "OK");
                return;
            }

            try
            {
                var token = await _auth.LoginAsync(email, password);

                if (string.IsNullOrWhiteSpace(token))
                {
                    await DisplayAlert("Ошибка", "Неверный email или пароль.", "OK");
                    return;
                }

                

                var services = App.Current!.Handler!.MauiContext!.Services;
                var mainPage = services.GetRequiredService<MainPage>();

                var window = this.Window ?? Application.Current!.Windows[0];
                window.Page = new NavigationPage(mainPage);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Исключение", ex.Message, "OK");
            }
        }


        private async void OnGoToRegisterClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(
                App.Current!.Handler!.MauiContext!.Services
                    .GetRequiredService<RegisterPage>());
        }
    }
}
