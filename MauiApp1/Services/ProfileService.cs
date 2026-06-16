using System.Net.Http.Json;

namespace MauiApp1.Services
{
    public class ProfileService
    {
        private readonly ApiClient _api;
        private readonly AuthService _auth;

        public ProfileService(ApiClient api, AuthService auth)
        {
            _api = api;
            _auth = auth;
        }

        public async Task<UserProfileDto?> GetProfileAsync(CancellationToken cancellationToken = default)
        {
            await _auth.ApplyTokenIfExistsAsync();

            var response = await _api.Http.GetAsync("api/profile", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw await ApiError.FromResponseAsync(response, "Не удалось загрузить профиль.", cancellationToken);
            }

            return await response.Content.ReadFromJsonAsync<UserProfileDto>(cancellationToken: cancellationToken);
        }
    }

    public class UserProfileDto
    {
        public string? Username { get; set; }
        public string? DisplayName { get; set; }
        public double OverallScore { get; set; }
        public double TodayPoints { get; set; }
        public int TotalSessions { get; set; }
        public int ExercisesTracked { get; set; }
        public string Readiness { get; set; } = string.Empty;
        public string? StrongestExercise { get; set; }
        public string? WeakestExercise { get; set; }
        public string? MostStableExercise { get; set; }
        public string? NeedsAttentionExercise { get; set; }
        public string Recommendation { get; set; } = string.Empty;
        public List<DailyActivityDto> DailyActivity { get; set; } = [];
        public List<ExerciseProgressDto> ExerciseProgress { get; set; } = [];
    }

    public class DailyActivityDto
    {
        public DateTime Date { get; set; }
        public double Points { get; set; }
        public int Sessions { get; set; }
    }

    public class ExerciseProgressDto
    {
        public string ExerciseType { get; set; } = string.Empty;
        public int CurrentLevel { get; set; }
        public double LastScore { get; set; }
        public double AverageScore { get; set; }
        public double BestScore { get; set; }
        public int SessionsCount { get; set; }
        public int SuccessStreak { get; set; }
        public int FailStreak { get; set; }
        public DateTime LastPlayedAt { get; set; }
        public string Trend { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
