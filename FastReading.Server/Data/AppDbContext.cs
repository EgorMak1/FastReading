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
        public DbSet<RunningWordsResult> RunningWordsResults { get; set; }
        public DbSet<FieldOfViewResult> FieldOfViewResults { get; set; }

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

            modelBuilder.Entity<FieldOfViewResult>(entity =>
            {
                entity.HasKey(x => x.Id);

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
        }
    }
}
