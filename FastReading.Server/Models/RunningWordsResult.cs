namespace FastReading.Server.Models
{
    public class RunningWordsResult
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public int TotalAttempts { get; set; }

        public int CorrectAnswers { get; set; }

        public int WrongAnswers { get; set; }

        public double AccuracyPercent { get; set; }

        public int FinalLevel { get; set; }

        public int FinalSpeedMilliseconds { get; set; }

        public DateTime CompletedAt { get; set; }
    }
}