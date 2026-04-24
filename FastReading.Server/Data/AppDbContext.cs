using FastReading.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace FastReading.Server.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<TrainingResult> TrainingResults { get; set; }
        public DbSet<ShulteResult> ShulteResults { get; set; }
        public DbSet<UserExerciseProgress> UserExerciseProgresses { get; set; }
        public DbSet<RunningWordsResult> RunningWordsResults { get; set; }
        public DbSet<FieldOfViewResult> FieldOfViewResults { get; set; }
        public DbSet<WordErasingResult> WordErasingResults { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.HasIndex(x => x.Username).IsUnique();
                entity.HasIndex(x => x.Email).IsUnique();

                entity.Property(x => x.Username)
                      .HasMaxLength(50)
                      .IsRequired();

                entity.Property(x => x.Email)
                      .HasMaxLength(255)
                      .IsRequired();

                entity.Property(x => x.PasswordHash)
                      .IsRequired();

                entity.Property(x => x.CreatedAt)
                      .IsRequired();
            });

            modelBuilder.Entity<TrainingResult>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.ExerciseType)
                      .HasMaxLength(50)
                      .IsRequired();

                entity.Property(x => x.DurationSeconds)
                      .IsRequired();

                entity.Property(x => x.CompletedAt)
                      .IsRequired();

                entity.HasOne(x => x.User)
                      .WithMany()
                      .HasForeignKey(x => x.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<RunningWordsResult>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.TotalAttempts)
                      .IsRequired();

                entity.Property(x => x.CorrectAnswers)
                      .IsRequired();

                entity.Property(x => x.WrongAnswers)
                      .IsRequired();

                entity.Property(x => x.AccuracyPercent)
                      .IsRequired();

                entity.Property(x => x.FinalLevel)
                      .IsRequired();

                entity.Property(x => x.FinalSpeedMilliseconds)
                      .IsRequired();

                entity.Property(x => x.CompletedAt)
                      .IsRequired();

                entity.HasOne<User>()
                      .WithMany()
                      .HasForeignKey(x => x.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ShulteResult>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.GridSize)
                      .IsRequired();

                entity.Property(x => x.NumbersCount)
                      .IsRequired();

                entity.Property(x => x.LevelBefore)
                      .IsRequired();

                entity.Property(x => x.LevelAfter)
                      .IsRequired();

                entity.Property(x => x.DurationSeconds)
                      .IsRequired();

                entity.Property(x => x.ErrorsCount)
                      .IsRequired();

                entity.Property(x => x.Score)
                      .IsRequired();

                entity.Property(x => x.CompletedAt)
                      .IsRequired();

                entity.HasOne<User>()
                      .WithMany()
                      .HasForeignKey(x => x.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<UserExerciseProgress>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.HasIndex(x => new { x.UserId, x.ExerciseType })
                      .IsUnique();

                entity.Property(x => x.ExerciseType)
                      .HasMaxLength(50)
                      .IsRequired();

                entity.Property(x => x.CurrentLevel)
                      .IsRequired();

                entity.Property(x => x.LastScore)
                      .IsRequired();

                entity.Property(x => x.AverageScore)
                      .IsRequired();

                entity.Property(x => x.BestScore)
                      .IsRequired();

                entity.Property(x => x.SessionsCount)
                      .IsRequired();

                entity.Property(x => x.SuccessStreak)
                      .IsRequired();

                entity.Property(x => x.FailStreak)
                      .IsRequired();

                entity.Property(x => x.LastPlayedAt)
                      .IsRequired();

                entity.HasOne<User>()
                      .WithMany()
                      .HasForeignKey(x => x.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<FieldOfViewResult>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.GridSize)
                      .IsRequired();

                entity.Property(x => x.TotalRounds)
                      .IsRequired();

                entity.Property(x => x.CorrectRounds)
                      .IsRequired();

                entity.Property(x => x.DetectedMismatchCount)
                      .IsRequired();

                entity.Property(x => x.MissedMismatchCount)
                      .IsRequired();

                entity.Property(x => x.FalseAlarmCount)
                      .IsRequired();

                entity.Property(x => x.AccuracyPercent)
                      .IsRequired();

                entity.Property(x => x.FinalLevel)
                      .IsRequired();

                entity.Property(x => x.FinalIntervalMilliseconds)
                      .IsRequired();

                entity.Property(x => x.CompletedAt)
                      .IsRequired();

                entity.HasOne<User>()
                      .WithMany()
                      .HasForeignKey(x => x.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<WordErasingResult>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.TextId)
                      .HasMaxLength(100)
                      .IsRequired();

                entity.Property(x => x.TextTitle)
                      .HasMaxLength(200)
                      .IsRequired();

                entity.Property(x => x.SpeedBeforeWpm)
                      .IsRequired();

                entity.Property(x => x.SpeedAfterWpm)
                      .IsRequired();

                entity.Property(x => x.SpeedDelta)
                      .IsRequired();

                entity.Property(x => x.CompletionType)
                      .HasMaxLength(20)
                      .IsRequired();

                entity.Property(x => x.CorrectAnswers)
                      .IsRequired();

                entity.Property(x => x.TotalQuestions)
                      .IsRequired();

                entity.Property(x => x.QuestionsSkipped)
                      .IsRequired();

                entity.Property(x => x.AccuracyPercent)
                      .IsRequired();

                entity.Property(x => x.ErasedWords)
                      .IsRequired();

                entity.Property(x => x.TotalWords)
                      .IsRequired();

                entity.Property(x => x.CompletedAt)
                      .IsRequired();

                entity.HasOne<User>()
                      .WithMany()
                      .HasForeignKey(x => x.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
