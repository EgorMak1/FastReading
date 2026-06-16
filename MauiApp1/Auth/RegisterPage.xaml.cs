using MauiApp1.Services;

using System.Net.Mail;

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
                await DisplayAlert("Ошибка", "Email и пароль должны быть заполнены.", "ОК");
                return;
            }

            if (!IsValidEmail(email))
            {
                await DisplayAlert("Ошибка", "Введите корректный email.", "ОК");
                return;
            }

            try
            {
                var result = await _auth.RegisterAsync(email, password);

                if (result == null)
                {
                    await DisplayAlert("Ошибка", "Регистрация не выполнена. Повторите попытку позже.", "ОК");
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

        private static bool IsValidEmail(string email)
        {
            try
            {
                var address = new MailAddress(email);
                return string.Equals(address.Address, email, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }
    }
}
