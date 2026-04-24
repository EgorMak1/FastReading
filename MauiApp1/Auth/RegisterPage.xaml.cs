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
                await DisplayAlert("Ошибка", "Введите email и пароль.", "ОК");
                return;
            }

            try
            {
                var result = await _auth.RegisterAsync(email, password);

                if (result == null)
                {
                    await DisplayAlert("Ошибка", "Пользователь уже существует или сервер недоступен.", "ОК");
                    return;
                }

                await DisplayAlert("Успех", $"Регистрация выполнена.\nЛогин: {result.Username}", "ОК");
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", ex.Message, "ОК");
            }
        }
    }
}
