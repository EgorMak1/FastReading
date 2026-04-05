namespace FastReading.Server.Models
{
    public class TrainingResult
    {
        // Уникальный идентификатор записи
        public Guid Id { get; set; }

        // Ссылка на пользователя (внешний ключ)
        public Guid UserId { get; set; }

        // Навигационное свойство — EF Core автоматически подгрузит пользователя
        public User User { get; set; } = null!;

        // Тип упражнения ("ShulteTable", "SpeedReading" и т.д.)
        public string ExerciseType { get; set; } = string.Empty;

        // Время прохождения в секундах
        public int DurationSeconds { get; set; }

        // Когда было выполнено упражнение
        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
    }
}