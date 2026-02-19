using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace MauiApp1.Services
{
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

        public async Task<bool> RegisterAsync(string email, string password)
        {
            var response = await _api.Http.PostAsJsonAsync("api/auth/register", new
            {
                email,
                password
            });

            return response.IsSuccessStatusCode;
        }
    }
}
