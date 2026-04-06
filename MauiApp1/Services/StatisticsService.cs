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

        // Сохраняем результат тренировки Шульте
        public async Task<bool> SaveResultAsync(string exerciseType, int durationSeconds)
        {
            await _auth.ApplyTokenIfExistsAsync();

            var response = await _api.Http.PostAsJsonAsync("api/statistics", new
            {
                exerciseType,
                durationSeconds
            });

            return response.IsSuccessStatusCode;
        }

        // Получаем список результатов Шульте
        public async Task<List<TrainingResultDto>> GetResultsAsync()
        {
            await _auth.ApplyTokenIfExistsAsync();

            var response = await _api.Http.GetAsync("api/statistics");

            if (!response.IsSuccessStatusCode)
            {
                return new List<TrainingResultDto>();
            }

            var results = await response.Content.ReadFromJsonAsync<List<TrainingResultDto>>();

            return results ?? new List<TrainingResultDto>();
        }

        // Сохраняем результат "Бегущих слов"
        public async Task<bool> SaveRunningWordsResultAsync(object result)
        {
            await _auth.ApplyTokenIfExistsAsync();

            var response = await _api.Http.PostAsJsonAsync("api/statistics/running-words", result);

            return response.IsSuccessStatusCode;
        }

        // Получаем результаты "Бегущих слов"
        public async Task<List<RunningWordsResultDto>> GetRunningWordsResultsAsync()
        {
            await _auth.ApplyTokenIfExistsAsync();

            var response = await _api.Http.GetAsync("api/statistics/running-words");

            if (!response.IsSuccessStatusCode)
            {
                return new List<RunningWordsResultDto>();
            }

            var results = await response.Content.ReadFromJsonAsync<List<RunningWordsResultDto>>();

            return results ?? new List<RunningWordsResultDto>();
        }
    }

    // DTO для статистики Шульте
    public class TrainingResultDto
    {
        public Guid Id { get; set; }
        public string ExerciseType { get; set; } = string.Empty;
        public int DurationSeconds { get; set; }
        public DateTime CompletedAt { get; set; }
    }

    // DTO для статистики "Бегущих слов"
    public class RunningWordsResultDto
    {
        public Guid Id { get; set; }
        public int TotalAttempts { get; set; }
        public int CorrectAnswers { get; set; }
        public int WrongAnswers { get; set; }
        public double AccuracyPercent { get; set; }
        public int FinalLevel { get; set; }
        public int FinalSpeedMilliseconds { get; set; }
        public DateTime CompletedAt { get; set; }
    }
}