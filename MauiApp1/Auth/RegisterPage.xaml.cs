namespace MauiApp1.Auth
{
    public partial class RegisterPage : ContentPage
    {
        public RegisterPage()
        {
            InitializeComponent();
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
                // Временно создаём HttpClient напрямую (для end-to-end регистрации).
                // Позже подключим DI и IHttpClientFactory правильно.
                using var client = new HttpClient
                {
                    BaseAddress = new Uri("http://158.160.179.55:5242/")
                };

                var payload = new
                {
                    email = email,
                    password = password
                };

                var json = System.Text.Json.JsonSerializer.Serialize(payload);
                using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await client.PostAsync("api/Auth/register", content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Ок", "Пользователь зарегистрирован.", "OK");
                    await Navigation.PopAsync();
                    return;
                }

                await DisplayAlert("Ошибка", $"Код: {response.StatusCode}\n{responseBody}", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Исключение", ex.Message, "OK");
            }
        }

    }
}
