namespace FastReading.Server.Models
{
    public class UserExerciseProgress
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string ExerciseType { get; set; } = string.Empty;
        public int CurrentLevel { get; set; }
        public double LastScore { get; set; }
        public double AverageScore { get; set; }
        public double BestScore { get; set; }
        public int SessionsCount { get; set; }
        public int SuccessStreak { get; set; }
        public int FailStreak { get; set; }
        public DateTime LastPlayedAt { get; set; } = DateTime.UtcNow;
    }
}
