namespace FastReading.Server.Models
{
    public class WordErasingResult
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
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
