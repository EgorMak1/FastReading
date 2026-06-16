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

        public async Task<bool> SaveResultAsync(string exerciseType, int durationSeconds)
        {
            await _auth.ApplyTokenIfExistsAsync();

            var response = await _api.Http.PostAsJsonAsync("api/statistics", new SaveResultRequest
            {
                ExerciseType = exerciseType,
                DurationSeconds = durationSeconds
            });

            if (!response.IsSuccessStatusCode)
            {
                throw await ApiError.FromResponseAsync(response, "Не удалось сохранить результат тренировки.");
            }

            return true;
        }

        public async Task<List<TrainingResultDto>> GetResultsAsync()
        {
            await _auth.ApplyTokenIfExistsAsync();

            var response = await _api.Http.GetAsync("api/statistics");

            if (!response.IsSuccessStatusCode)
            {
                throw await ApiError.FromResponseAsync(response, "Не удалось загрузить статистику.");
            }

            var results = await response.Content.ReadFromJsonAsync<List<TrainingResultDto>>();

            return results ?? new List<TrainingResultDto>();
        }

        public async Task<bool> SaveRunningWordsResultAsync(RunningWordsResultRequest result)
        {
            await _auth.ApplyTokenIfExistsAsync();

            var response = await _api.Http.PostAsJsonAsync("api/statistics/running-words", result);

            if (!response.IsSuccessStatusCode)
            {
                throw await ApiError.FromResponseAsync(response, "Не удалось сохранить результат тренировки.");
            }

            return true;
        }

        public async Task<List<RunningWordsResultDto>> GetRunningWordsResultsAsync()
        {
            await _auth.ApplyTokenIfExistsAsync();

            var response = await _api.Http.GetAsync("api/statistics/running-words");

            if (!response.IsSuccessStatusCode)
            {
                throw await ApiError.FromResponseAsync(response, "Не удалось загрузить статистику упражнения.");
            }

            var results = await response.Content.ReadFromJsonAsync<List<RunningWordsResultDto>>();

            return results ?? new List<RunningWordsResultDto>();
        }

        public async Task<bool> SaveShulteResultAsync(ShulteResultRequest result)
        {
            await _auth.ApplyTokenIfExistsAsync();

            var response = await _api.Http.PostAsJsonAsync("api/statistics/shulte", result);

            if (!response.IsSuccessStatusCode)
            {
                throw await ApiError.FromResponseAsync(response, "Не удалось сохранить результат тренировки.");
            }

            return true;
        }

        public async Task<List<ShulteResultDto>> GetShulteResultsAsync()
        {
            await _auth.ApplyTokenIfExistsAsync();

            var response = await _api.Http.GetAsync("api/statistics/shulte");

            if (!response.IsSuccessStatusCode)
            {
                throw await ApiError.FromResponseAsync(response, "Не удалось загрузить статистику упражнения.");
            }

            var results = await response.Content.ReadFromJsonAsync<List<ShulteResultDto>>();

            return results ?? new List<ShulteResultDto>();
        }

        public async Task<bool> SaveFieldOfViewResultAsync(FieldOfViewResultRequest result)
        {
            await _auth.ApplyTokenIfExistsAsync();

            var response = await _api.Http.PostAsJsonAsync("api/statistics/field-of-view", result);

            if (!response.IsSuccessStatusCode)
            {
                throw await ApiError.FromResponseAsync(response, "Не удалось сохранить результат тренировки.");
            }

            return true;
        }

        public async Task<List<FieldOfViewResultDto>> GetFieldOfViewResultsAsync()
        {
            await _auth.ApplyTokenIfExistsAsync();

            var response = await _api.Http.GetAsync("api/statistics/field-of-view");

            if (!response.IsSuccessStatusCode)
            {
                throw await ApiError.FromResponseAsync(response, "Не удалось загрузить статистику упражнения.");
            }

            var results = await response.Content.ReadFromJsonAsync<List<FieldOfViewResultDto>>();

            return results ?? new List<FieldOfViewResultDto>();
        }

        public async Task<bool> SaveWordErasingResultAsync(WordErasingResultRequest result)
        {
            await _auth.ApplyTokenIfExistsAsync();

            var response = await _api.Http.PostAsJsonAsync("api/statistics/word-erasing", result);

            if (!response.IsSuccessStatusCode)
            {
                throw await ApiError.FromResponseAsync(response, "Не удалось сохранить результат тренировки.");
            }

            return true;
        }

        public async Task<List<WordErasingResultDto>> GetWordErasingResultsAsync()
        {
            await _auth.ApplyTokenIfExistsAsync();

            var response = await _api.Http.GetAsync("api/statistics/word-erasing");

            if (!response.IsSuccessStatusCode)
            {
                throw await ApiError.FromResponseAsync(response, "Не удалось загрузить статистику упражнения.");
            }

            var results = await response.Content.ReadFromJsonAsync<List<WordErasingResultDto>>();

            return results ?? new List<WordErasingResultDto>();
        }
    }

    public class SaveResultRequest
    {
        public string ExerciseType { get; set; } = string.Empty;
        public int DurationSeconds { get; set; }
    }

    public class TrainingResultDto
    {
        public Guid Id { get; set; }
        public string ExerciseType { get; set; } = string.Empty;
        public int DurationSeconds { get; set; }
        public DateTime CompletedAt { get; set; }
    }

    public class RunningWordsResultRequest
    {
        public int TotalAttempts { get; set; }
        public int CorrectAnswers { get; set; }
        public int WrongAnswers { get; set; }
        public double AccuracyPercent { get; set; }
        public int FinalLevel { get; set; }
        public int FinalSpeedMilliseconds { get; set; }
    }

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

    public class ShulteResultRequest
    {
        public int GridSize { get; set; }
        public int NumbersCount { get; set; }
        public int LevelBefore { get; set; }
        public int LevelAfter { get; set; }
        public int DurationSeconds { get; set; }
        public int ErrorsCount { get; set; }
        public double Score { get; set; }
    }

    public class ShulteResultDto
    {
        public Guid Id { get; set; }
        public int GridSize { get; set; }
        public int NumbersCount { get; set; }
        public int LevelBefore { get; set; }
        public int LevelAfter { get; set; }
        public int DurationSeconds { get; set; }
        public int ErrorsCount { get; set; }
        public double Score { get; set; }
        public DateTime CompletedAt { get; set; }
    }

    public class FieldOfViewResultRequest
    {
        public int GridSize { get; set; }
        public int TotalRounds { get; set; }
        public int CorrectRounds { get; set; }
        public int DetectedMismatchCount { get; set; }
        public int MissedMismatchCount { get; set; }
        public int FalseAlarmCount { get; set; }
        public double AccuracyPercent { get; set; }
        public int FinalLevel { get; set; }
        public int FinalIntervalMilliseconds { get; set; }
    }

    public class FieldOfViewResultDto
    {
        public Guid Id { get; set; }
        public int GridSize { get; set; }
        public int TotalRounds { get; set; }
        public int CorrectRounds { get; set; }
        public int DetectedMismatchCount { get; set; }
        public int MissedMismatchCount { get; set; }
        public int FalseAlarmCount { get; set; }
        public double AccuracyPercent { get; set; }
        public int FinalLevel { get; set; }
        public int FinalIntervalMilliseconds { get; set; }
        public DateTime CompletedAt { get; set; }
    }

    public class WordErasingResultRequest
    {
        public string TextId { get; set; } = string.Empty;
        public string TextTitle { get; set; } = string.Empty;
        public int SpeedBeforeWpm { get; set; }
        public int SpeedAfterWpm { get; set; }
        public int SpeedDelta { get; set; }
        public string CompletionType { get; set; } = string.Empty;
        public int CorrectAnswers { get; set; }
        public int TotalQuestions { get; set; }
        public bool QuestionsSkipped { get; set; }
        public double AccuracyPercent { get; set; }
        public int ErasedWords { get; set; }
        public int TotalWords { get; set; }
    }

    public class WordErasingResultDto
    {
        public Guid Id { get; set; }
        public string TextId { get; set; } = string.Empty;
        public string TextTitle { get; set; } = string.Empty;
        public int SpeedBeforeWpm { get; set; }
        public int SpeedAfterWpm { get; set; }
        public int SpeedDelta { get; set; }
        public string CompletionType { get; set; } = string.Empty;
        public int CorrectAnswers { get; set; }
        public int TotalQuestions { get; set; }
        public bool QuestionsSkipped { get; set; }
        public double AccuracyPercent { get; set; }
        public int ErasedWords { get; set; }
        public int TotalWords { get; set; }
        public DateTime CompletedAt { get; set; }
    }
}
