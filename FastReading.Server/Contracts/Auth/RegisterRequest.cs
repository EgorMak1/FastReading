namespace FastReading.Server.Contracts.Auth
{
    // Контракт регистрации пользователя.
    // Это модель данных, которую клиент (MauiApp1) отправляет на сервер.
    public class RegisterRequest
    {
        // Email пользователя (будет использоваться как логин)
        public string Email { get; set; } = string.Empty;

        // Пароль в открытом виде.
        // В базу мы его сохранять НЕ будем — только хеш.
        public string Password { get; set; } = string.Empty;
    }
}
