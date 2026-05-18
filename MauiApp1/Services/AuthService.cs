using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MauiApp1.Services
{
    public sealed class RegisterResponse
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
    }

    public class AuthService
    {
        private const string TokenKey = "access_token";
        private readonly ApiClient _api;

        public AuthService(ApiClient api)
        {
            _api = api;
        }

        public async Task<string?> LoginAsync(string email, string password)
        {
            try
            {
                var response = await _api.Http.PostAsJsonAsync("api/auth/login", new
                {
                    email,
                    password
                });

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var data = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (data == null || string.IsNullOrWhiteSpace(data.AccessToken))
                {
                    return null;
                }

                await SecureStorage.Default.SetAsync(TokenKey, data.AccessToken);

                // сразу выставляем токен в HttpClient
                _api.Http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", data.AccessToken);

                return data.AccessToken;
            }
            catch (HttpRequestException)
            {
                return null;
            }
            catch (TaskCanceledException)
            {
                return null;
            }
            catch (System.Text.Json.JsonException)
            {
                return null;
            }
        }

        public async Task<string?> GetAccessTokenAsync()
        {
            return await SecureStorage.Default.GetAsync(TokenKey);
        }

        public async Task ApplyTokenIfExistsAsync()
        {
            var token = await GetAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(token))
            {
                return;
            }

            _api.Http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<bool> TryApplySavedTokenAsync()
        {
            try
            {
                var token = await GetAccessTokenAsync();
                if (string.IsNullOrWhiteSpace(token))
                {
                    return false;
                }

                if (IsJwtExpired(token))
                {
                    await LogoutAsync();
                    return false;
                }

                _api.Http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task LogoutAsync()
        {
            SecureStorage.Default.Remove(TokenKey);
            _api.Http.DefaultRequestHeaders.Authorization = null;
            await Task.CompletedTask;
        }

        private sealed class LoginResponse
        {
            public string AccessToken { get; set; } = string.Empty;
        }

        private static bool IsJwtExpired(string token)
        {
            try
            {
                var parts = token.Split('.');
                if (parts.Length < 2)
                {
                    return true;
                }

                var payloadJson = Encoding.UTF8.GetString(Base64UrlDecode(parts[1]));
                using var payload = JsonDocument.Parse(payloadJson);

                if (!payload.RootElement.TryGetProperty("exp", out var expiresAtElement) ||
                    !expiresAtElement.TryGetInt64(out var expiresAt))
                {
                    return false;
                }

                return DateTimeOffset.FromUnixTimeSeconds(expiresAt) <= DateTimeOffset.UtcNow.AddMinutes(1);
            }
            catch
            {
                return true;
            }
        }

        private static byte[] Base64UrlDecode(string value)
        {
            var padded = value
                .Replace('-', '+')
                .Replace('_', '/');

            padded = padded.PadRight(padded.Length + (4 - padded.Length % 4) % 4, '=');

            return Convert.FromBase64String(padded);
        }

        public async Task<RegisterResponse?> RegisterAsync(string email, string password)
        {
            var response = await _api.Http.PostAsJsonAsync("api/auth/register", new
            {
                email,
                password
            });

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<RegisterResponse>();
        }
    }
}
