namespace FastReading.Server.Models
{
    public class FieldOfViewResult
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
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
}
