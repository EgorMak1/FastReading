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
            // 1) Читаем данные из UI
            var email = EmailEntry.Text?.Trim();
            var password = PasswordEntry.Text;

            // 2) Минимальная валидация на клиенте (чтобы не отправлять пустые данные на сервер)
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("Ошибка", "Введите Email и Password.", "OK");
                return;
            }

            try
            {
                // 3) Создаём HttpClient
                // Сейчас временно создаём напрямую, чтобы быстро проверить регистрацию end-to-end.
                // Позже заменим на HttpClient из DI (через IHttpClientFactory), чтобы:
                // - не дублировать BaseAddress
                // - централизованно настраивать заголовки/таймауты/токены
                using var client = new HttpClient
                {
                    BaseAddress = new Uri("http://158.160.179.55:5242/")
                };

                // 4) Формируем payload запроса регистрации
                var payload = new
                {
                    email = email,
                    password = password
                };

                // 5) Сериализуем в JSON и отправляем POST /api/Auth/register
                var json = System.Text.Json.JsonSerializer.Serialize(payload);
                using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await client.PostAsync("api/Auth/register", content);

                // 6) Читаем тело ответа (оно понадобится и для успеха, и для отображения ошибки)
                var responseBody = await response.Content.ReadAsStringAsync();

                // 7) Успех: парсим JSON и показываем назначенный username
                if (response.IsSuccessStatusCode)
                {
                    // Ожидаемый ответ сервера:
                    // { "userId": "...", "email": "...", "username": "user_xxxxxxxx" }
                    using var doc = System.Text.Json.JsonDocument.Parse(responseBody);
                    var root = doc.RootElement;

                    string? username = null;
                    if (root.TryGetProperty("username", out var usernameProp))
                    {
                        username = usernameProp.GetString();
                    }

                    await DisplayAlert(
                        "Регистрация успешна",
                        $"Ваше имя пользователя: {username}\nВы сможете изменить его позже.",
                        "OK");

                    // Возвращаемся назад на предыдущую страницу (MainPage)
                    await Navigation.PopAsync();
                    return;
                }

                // 8) Ошибка: показываем код и сообщение сервера (например 409 Conflict)
                await DisplayAlert("Ошибка", $"Код: {response.StatusCode}\n{responseBody}", "OK");
            }
            catch (Exception ex)
            {
                // 9) Любое исключение на клиенте (нет сети, сервер недоступен, неверный URL и т.д.)
                await DisplayAlert("Исключение", ex.Message, "OK");
            }
        }
    }
}
