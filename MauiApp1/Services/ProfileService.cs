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

        public async Task<UserProfileDto?> GetProfileAsync()
        {
            await _auth.ApplyTokenIfExistsAsync();

            var response = await _api.Http.GetAsync("api/profile");
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<UserProfileDto>();
        }
    }

    public class UserProfileDto
    {
        public double OverallScore { get; set; }
        public double TodayPoints { get; set; }
        public int TotalSessions { get; set; }
        public int ExercisesTracked { get; set; }
        public string? StrongestExercise { get; set; }
        public string? WeakestExercise { get; set; }
        public List<ExerciseProgressDto> ExerciseProgress { get; set; } = [];
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
    }
}
