using MauiApp1.Services;

namespace MauiApp1.Auth
{
    public partial class RegisterPage : ContentPage
    {
        private readonly AuthService _auth;

        public RegisterPage(AuthService auth)
        {
            InitializeComponent();
            _auth = auth;
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
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
                var result = await _auth.RegisterAsync(email, password);

                if (result == null)
                {
                    await DisplayAlert("Ошибка", "Пользователь уже существует или сервер недоступен.", "OK");
                    return;
                }

                await DisplayAlert("Успех", $"Регистрация выполнена.\nUsername: {result.Username}", "OK");
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Исключение", ex.Message, "OK");
            }
        }
    }
}
