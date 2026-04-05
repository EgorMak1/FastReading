using System.Net.Http.Json;

namespace MauiApp1.Services
{
    public class StatisticsService
    {
        private readonly ApiClient _api;
        private readonly AuthService _auth;

        public StatisticsService(ApiClient api, AuthService auth)
        {
            _api = api;
            _auth = auth;
        }

        // Сохраняем результат тренировки на сервер
        public async Task<bool> SaveResultAsync(string exerciseType, int durationSeconds)
        {
            // Применяем токен перед запросом
            await _auth.ApplyTokenIfExistsAsync();

            var response = await _api.Http.PostAsJsonAsync("api/statistics", new
            {
                exerciseType,
                durationSeconds
            });

            return response.IsSuccessStatusCode;
        }

        // Получаем список результатов с сервера
        public async Task<List<TrainingResultDto>> GetResultsAsync()
        {
            // Применяем токен перед запросом
            await _auth.ApplyTokenIfExistsAsync();

            var response = await _api.Http.GetAsync("api/statistics");

            if (!response.IsSuccessStatusCode)
            {
                return new List<TrainingResultDto>();
            }

            var results = await response.Content
                .ReadFromJsonAsync<List<TrainingResultDto>>();

            return results ?? new List<TrainingResultDto>();
        }
    }

    // DTO — модель данных которую возвращает сервер
    public class TrainingResultDto
    {
        public Guid Id { get; set; }
        public string ExerciseType { get; set; } = string.Empty;
        public int DurationSeconds { get; set; }
        public DateTime CompletedAt { get; set; }
    }
}