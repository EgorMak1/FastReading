namespace FastReading.Server.Models
{
    public class ShulteResult
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public int GridSize { get; set; }
        public int NumbersCount { get; set; }
        public int LevelBefore { get; set; }
        public int LevelAfter { get; set; }
        public int DurationSeconds { get; set; }
        public int ErrorsCount { get; set; }
        public double Score { get; set; }
        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
    }
}
