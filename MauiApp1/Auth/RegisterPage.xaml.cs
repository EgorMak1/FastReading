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
                var token = await _auth.LoginAsync(email, password);

                if (string.IsNullOrWhiteSpace(token))
                {
                    await DisplayAlert("Ошибка", "Неверный email или пароль (или сервер недоступен).", "OK");
                    return;
                }

                await DisplayAlert("Успех", "Токен получен и сохранён.", "OK");
               

                await _auth.ApplyTokenIfExistsAsync();
                await DisplayAlert("Token applied", "Authorization header установлен", "OK");


                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Исключение", ex.Message, "OK");
            }
        }

    }
}
